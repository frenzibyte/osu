// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2.Leaderboards
{
    public partial class LeaderboardSection : CompositeDrawable
    {
        private const float corner_radius = 10;

        public LeaderboardSection()
        {
            Width = 600;
            Height = 500;
            Y = 400;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            var shear = new Vector2(OsuGame.SHEAR, 0);

            Masking = true;
            CornerRadius = corner_radius;
            Shear = new Vector2(OsuGame.SHEAR, 0);
            Margin = new MarginPadding { Left = -corner_radius };

            InternalChildren = new[]
            {
                new LeaderboardSectionHeader
                {
                    RelativeSizeAxes = Axes.X,
                },
            };
        }
    }
}
