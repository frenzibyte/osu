// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Scoring
{
    public interface IScoreInfo : IHasOnlineID<long>, IEquatable<IScoreInfo>
    {
        IUser User { get; }

        long TotalScore { get; }

        int MaxCombo { get; }

        double Accuracy { get; }

        bool HasReplay { get; }

        DateTimeOffset Date { get; }

        double? PP { get; }

        IBeatmapInfo Beatmap { get; }

        int RulesetID { get; }

        ScoreRank Rank { get; }

        string Hash { get; }

        APIMod[] Mods { get; }

        // Statistics is also missing. This can be reconsidered once changes in serialisation have been completed.
    }
}
