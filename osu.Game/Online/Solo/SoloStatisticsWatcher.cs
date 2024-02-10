// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.Solo
{
    /// <summary>
    /// A persistent component that binds to the spectator server and API in order to deliver updates about the logged in user's gameplay statistics.
    /// This also handles updating the <see cref="IAPIProvider.Statistics"/> bindable based on changes to the local user, game-wide ruleset, or processed gameplay scores.
    /// </summary>
    public partial class SoloStatisticsWatcher : Component
    {
        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public Bindable<UserStatistics?> Statistics { get; } = new Bindable<UserStatistics?>();

        private readonly Dictionary<long, StatisticsUpdateCallback> callbacks = new Dictionary<long, StatisticsUpdateCallback>();
        private long? lastProcessedScoreId;

        private readonly Dictionary<string, UserStatistics> latestStatistics = new Dictionary<string, UserStatistics>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Statistics.BindValueChanged(v =>
            {
                if (api.LocalUser.Value != null && v.NewValue != null)
                    api.LocalUser.Value.Statistics = v.NewValue;
            });

            api.LocalUser.BindValueChanged(_ => onUserChanged(), true);
            ruleset.BindValueChanged(_ => onRulesetChanged(), true);

            spectatorClient.OnUserScoreProcessed += userScoreProcessed;
        }

        /// <summary>
        /// Registers for a user statistics update after the given <paramref name="score"/> has been processed server-side.
        /// </summary>
        /// <param name="score">The score to listen for the statistics update for.</param>
        /// <param name="onUpdateReady">The callback to be invoked once the statistics update has been prepared.</param>
        /// <returns>An <see cref="IDisposable"/> representing the subscription. Disposing it is equivalent to unsubscribing from future notifications.</returns>
        public IDisposable RegisterForStatisticsUpdateAfter(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady)
        {
            Schedule(() =>
            {
                if (!api.IsLoggedIn)
                    return;

                if (!score.Ruleset.IsLegacyRuleset() || score.OnlineID <= 0)
                    return;

                var callback = new StatisticsUpdateCallback(score, onUpdateReady);

                if (lastProcessedScoreId == score.OnlineID)
                {
                    requestStatisticsUpdate(api.LocalUser.Value.Id, callback.Score.Ruleset, callback);
                    return;
                }

                callbacks.Add(score.OnlineID, callback);
            });

            return new InvokeOnDisposal(() => Schedule(() => callbacks.Remove(score.OnlineID)));
        }

        private void onUserChanged() => Schedule(() =>
        {
            callbacks.Clear();
            lastProcessedScoreId = null;
            latestStatistics.Clear();

            updateStatisticsBindable();
        });

        private void onRulesetChanged() => Schedule(updateStatisticsBindable);

        private void updateStatisticsBindable()
        {
            Statistics.Value = null;

            if (api.LocalUser.Value == null || api.LocalUser.Value.OnlineID <= 1 || ruleset.Value?.IsLegacyRuleset() != true)
            {
                Statistics.Value = new UserStatistics();
                return;
            }

            if (!latestStatistics.TryGetValue(ruleset.Value.ShortName, out var statistics))
                requestStatisticsUpdate(api.LocalUser.Value.OnlineID, ruleset.Value);
            else
                Statistics.Value = statistics;
        }

        private void userScoreProcessed(int userId, long scoreId)
        {
            if (userId != api.LocalUser.Value?.OnlineID)
                return;

            lastProcessedScoreId = scoreId;

            if (!callbacks.TryGetValue(scoreId, out var callback))
                return;

            requestStatisticsUpdate(userId, callback.Score.Ruleset, callback);
            callbacks.Remove(scoreId);
        }

        private void requestStatisticsUpdate(int userId, IRulesetInfo ruleset, StatisticsUpdateCallback? callback = null)
        {
            var request = new GetUserRequest(userId, ruleset);
            request.Success += user => Schedule(() => dispatchStatisticsUpdate(user.Statistics, ruleset, callback));
            api.Queue(request);
        }

        private void dispatchStatisticsUpdate(UserStatistics updatedStatistics, IRulesetInfo statisticsRuleset, StatisticsUpdateCallback? callback = null)
        {
            if (callback != null)
            {
                latestStatistics.TryGetValue(statisticsRuleset.ShortName, out UserStatistics? latestRulesetStatistics);
                latestRulesetStatistics ??= new UserStatistics();

                var update = new SoloStatisticsUpdate(callback.Score, latestRulesetStatistics, updatedStatistics);
                callback.OnUpdateReady.Invoke(update);
            }

            latestStatistics[statisticsRuleset.ShortName] = updatedStatistics;

            // the statistics ruleset may not match the game-wide ruleset bindable (assume player changed their ruleset before the score finished processing).
            if (statisticsRuleset.ShortName == ruleset.Value.ShortName)
                Statistics.Value = updatedStatistics;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (spectatorClient.IsNotNull())
                spectatorClient.OnUserScoreProcessed -= userScoreProcessed;

            base.Dispose(isDisposing);
        }

        private class StatisticsUpdateCallback
        {
            public ScoreInfo Score { get; }
            public Action<SoloStatisticsUpdate> OnUpdateReady { get; }

            public StatisticsUpdateCallback(ScoreInfo score, Action<SoloStatisticsUpdate> onUpdateReady)
            {
                Score = score;
                OnUpdateReady = onUpdateReady;
            }
        }
    }
}
