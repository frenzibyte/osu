// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.Collections.Generic;
using osu.Game.Scoring;

namespace osu.Game.Online.Leaderboards
{
    public interface ILeaderboard
    {
        IEnumerable<IScoreInfo> Scores { get; }
    }
}
