// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserScoreAggregate : IScoreInfo
    {
        [JsonProperty("attempts")]
        public int TotalAttempts { get; set; }

        [JsonProperty("completed")]
        public int CompletedBeatmaps { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty(@"pp")]
        public double? PP { get; set; }

        [JsonProperty(@"room_id")]
        public long RoomID { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty(@"user_id")]
        public long UserID { get; set; }

        [JsonProperty("user")]
        public APIUser User { get; set; }

        [JsonProperty("position")]
        public int? Position { get; set; }

        public ScoreInfo CreateScoreInfo() =>
            new ScoreInfo
            {
                Accuracy = Accuracy,
                PP = PP,
                TotalScore = TotalScore,
                User = User,
                Position = Position,
                Mods = Array.Empty<Mod>()
            };

        long IHasOnlineID<long>.OnlineID => 0;
        IEnumerable<INamedFileUsage> IHasNamedFiles.Files => Enumerable.Empty<INamedFileUsage>();
        bool IScoreInfo.HasReplay => false;
        DateTimeOffset IScoreInfo.Date => DateTimeOffset.Now;
        IBeatmapInfo IScoreInfo.Beatmap => null;
        IRulesetInfo IScoreInfo.Ruleset => null;
        ScoreRank IScoreInfo.Rank => ScoreRank.X;
        int IScoreInfo.MaxCombo => 0; // todo: where the fuck is max combo.
        IUser IScoreInfo.User => User;
    }
}
