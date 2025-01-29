// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonSongProgressGraph : SegmentedGraph<int>
    {
        private const int tier_count = 5;

        private const int display_granularity = 200;

        public IEnumerable<HitObject> Objects
        {
            set => Values = BeatmapExtensions.CalculateDensity(value, display_granularity);
        }

        public ArgonSongProgressGraph()
            : base(tier_count)
        {
            var colours = new List<Colour4>();

            for (int i = 0; i < tier_count; i++)
                colours.Add(OsuColour.Gray(0.2f).Opacity(0.1f));

            TierColours = colours;
        }
    }
}
