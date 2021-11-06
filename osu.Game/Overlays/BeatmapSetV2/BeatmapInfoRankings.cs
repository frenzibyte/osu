// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2
{
    public class BeatmapInfoRankings : BeatmapInfoSection
    {
        private const int spacing = 15;

        [Resolved]
        private IBindable<APIBeatmap> beatmap { get; set; }

        private readonly Bindable<IRulesetInfo> ruleset = new Bindable<IRulesetInfo>();
        private readonly Bindable<BeatmapLeaderboardScope> scope = new Bindable<BeatmapLeaderboardScope>(BeatmapLeaderboardScope.Global);
        private readonly IBindable<User> user = new Bindable<User>();

        private ScoreTable scoreTable;
        private FillFlowContainer topScoresContainer;
        private LoadingLayer loading;
        private LeaderboardModSelector modSelector;
        private NoScoresPlaceholder noScoresPlaceholder;
        private NotSupporterPlaceholder notSupporterPlaceholder;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        private GetScoresRequest getScoresRequest;

        private CancellationTokenSource loadCancellationSource;

        protected APIScoresCollection Scores
        {
            set => Schedule(() =>
            {
                loadCancellationSource?.Cancel();
                loadCancellationSource = new CancellationTokenSource();

                topScoresContainer.Clear();
                scoreTable.ClearScores();
                scoreTable.Hide();

                if (value?.Scores.Any() != true)
                    return;

                scoreManager.OrderByTotalScoreAsync(value.Scores.Select(s => s.CreateScoreInfo(rulesets)).ToArray(), loadCancellationSource.Token)
                            .ContinueWith(ordered => Schedule(() =>
                            {
                                if (loadCancellationSource.IsCancellationRequested)
                                    return;

                                var topScore = ordered.Result.First();

                                scoreTable.DisplayScores(ordered.Result, topScore.BeatmapInfo?.Status.GrantsPerformancePoints() == true);
                                scoreTable.Show();

                                var userScore = value.UserScore;
                                var userScoreInfo = userScore?.Score.CreateScoreInfo(rulesets);

                                topScoresContainer.Add(new DrawableTopScore(topScore));

                                if (userScoreInfo != null && userScoreInfo.OnlineScoreID != topScore.OnlineScoreID)
                                    topScoresContainer.Add(new DrawableTopScore(userScoreInfo, userScore.Position));
                            }), TaskContinuationOptions.OnlyOnRanToCompletion);
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = 50 },
                    Margin = new MarginPadding { Vertical = 20 },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, spacing),
                            Children = new Drawable[]
                            {
                                new LeaderboardScopeSelector
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Current = { BindTarget = scope }
                                },
                                modSelector = new LeaderboardModSelector
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Ruleset = { BindTarget = ruleset }
                                }
                            }
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Top = spacing },
                            Children = new Drawable[]
                            {
                                noScoresPlaceholder = new NoScoresPlaceholder
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                    Margin = new MarginPadding { Vertical = 10 }
                                },
                                notSupporterPlaceholder = new NotSupporterPlaceholder
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Alpha = 0,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, spacing),
                                    Children = new Drawable[]
                                    {
                                        topScoresContainer = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 5),
                                        },
                                        scoreTable = new ScoreTable
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        }
                                    }
                                },
                            }
                        }
                    },
                },
                loading = new LoadingLayer()
            });

            user.BindTo(api.LocalUser);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            scope.BindValueChanged(_ => getScores());
            ruleset.BindValueChanged(_ => getScores());

            modSelector.SelectedMods.CollectionChanged += (_, __) => getScores();

            beatmap.BindValueChanged(onBeatmapChanged);
            user.BindValueChanged(onUserChanged, true);
        }

        private void onBeatmapChanged(ValueChangedEvent<APIBeatmap> beatmap)
        {
            var beatmapRuleset = beatmap.NewValue?.Ruleset;

            if (ruleset.Value?.OnlineID == beatmapRuleset?.OnlineID)
            {
                modSelector.DeselectAll();
                ruleset.TriggerChange();
            }
            else
                ruleset.Value = beatmapRuleset;

            scope.Value = BeatmapLeaderboardScope.Global;
        }

        private void onUserChanged(ValueChangedEvent<User> user)
        {
            if (modSelector.SelectedMods.Any())
                modSelector.DeselectAll();
            else
                getScores();

            modSelector.FadeTo(userIsSupporter ? 1 : 0);
        }

        private void getScores()
        {
            getScoresRequest?.Cancel();
            getScoresRequest = null;

            noScoresPlaceholder.Hide();

            if (beatmap.Value == null || beatmap.Value.OnlineID <= 0 || (beatmap.Value?.BeatmapSet as IBeatmapSetOnlineInfo)?.Status <= BeatmapSetOnlineStatus.Pending)
            {
                Scores = null;
                Hide();
                return;
            }

            if (scope.Value != BeatmapLeaderboardScope.Global && !userIsSupporter)
            {
                Scores = null;
                notSupporterPlaceholder.Show();

                loading.Hide();
                loading.FinishTransforms();
                return;
            }

            notSupporterPlaceholder.Hide();

            Show();
            loading.Show();

            getScoresRequest = new GetScoresRequest(beatmap.Value, beatmap.Value.Ruleset, scope.Value, modSelector.SelectedMods);
            getScoresRequest.Success += scores =>
            {
                loading.Hide();
                loading.FinishTransforms();

                Scores = scores;

                if (!scores.Scores.Any())
                    noScoresPlaceholder.ShowWithScope(scope.Value);
            };

            api.Queue(getScoresRequest);
        }

        private bool userIsSupporter => api.IsLoggedIn && api.LocalUser.Value.IsSupporter;
    }
}
