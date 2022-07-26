// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Scoring;
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

        long IHasOnlineID<long>.OnlineID => Score.OnlineID;

        bool IEquatable<IScoreInfo>.Equals(IScoreInfo other) => Score.Equals(other);

        IUser IScoreInfo.User => ((IScoreInfo)Score).User;

        long IScoreInfo.TotalScore => ((IScoreInfo)Score).TotalScore;

        int IScoreInfo.MaxCombo => Score.MaxCombo;

        double IScoreInfo.Accuracy => Score.Accuracy;

        bool IScoreInfo.HasReplay => Score.HasReplay;

        DateTimeOffset IScoreInfo.Date => ((IScoreInfo)Score).Date;

        double? IScoreInfo.PP => Score.PP;

        IBeatmapInfo IScoreInfo.Beatmap => ((IScoreInfo)Score).Beatmap;

        int IScoreInfo.RulesetID => Score.RulesetID;

        ScoreRank IScoreInfo.Rank => Score.Rank;

        string IScoreInfo.Hash => Score.Hash;

        APIMod[] IScoreInfo.Mods => Score.Mods;

        Dictionary<HitResult, int> IScoreInfo.Statistics => Score.Statistics;

        #endregion
    }
}
