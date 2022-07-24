// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class SoloScoreInfoWithPosition : IScoreInfo
    {
        [JsonProperty(@"position")]
        public int? Position;

        [JsonProperty(@"score")]
        public SoloScoreInfo Score;

        #region IScoreInfo

        public long OnlineID => Score.OnlineID;

        public bool Equals(IScoreInfo other) => Score.Equals(other);

        public IUser User => ((IScoreInfo)Score).User;

        public long TotalScore => ((IScoreInfo)Score).TotalScore;

        public int MaxCombo => Score.MaxCombo;

        public double Accuracy => Score.Accuracy;

        public bool HasReplay => Score.HasReplay;

        public DateTimeOffset Date => ((IScoreInfo)Score).Date;

        public double? PP => Score.PP;

        public IBeatmapInfo Beatmap => ((IScoreInfo)Score).Beatmap;

        public IRulesetInfo Ruleset => ((IScoreInfo)Score).Ruleset;

        public ScoreRank Rank => Score.Rank;

        public string Hash => Score.Hash;

        #endregion
    }
}
