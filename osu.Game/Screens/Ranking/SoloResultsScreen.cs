// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public class SoloResultsScreen : ResultsScreen
    {
        private GetScoresRequest getScoreRequest;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        public SoloResultsScreen(IScoreInfo score, bool allowRetry)
            : base(score, allowRetry)
        {
        }

        protected override APIRequest FetchScores(Action<IEnumerable<IScoreInfo>> scoresCallback)
        {
            if (Score.Beatmap.OnlineID <= 0 ||
                (Score.Beatmap as BeatmapInfo)?.Status <= BeatmapOnlineStatus.Pending ||
                (Score.Beatmap as IBeatmapSetOnlineInfo)?.Status <= BeatmapOnlineStatus.Pending)
                return null;

            getScoreRequest = new GetScoresRequest(Score.Beatmap, Score.GetRuleset(rulesets));
            getScoreRequest.Success += r => scoresCallback?.Invoke(r.Scores.Where(s => s.OnlineID != Score.OnlineID));
            return getScoreRequest;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            getScoreRequest?.Cancel();
        }
    }
}
