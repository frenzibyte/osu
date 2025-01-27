// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapStatisticsWedge : CompositeDrawable
    {
        /// Todo: move this const out to song select when more new design elements are implemented for the beatmap details area, since it applies to text alignment of various elements
        private const float text_margin = 62;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        public BeatmapStatisticsWedge()
        {
            X = 340f;
            Y = 320f;
            Width = 250f;
            Height = 50f;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 10f,
                    Masking = true,
                    Shear = shear,
                    Margin = new MarginPadding { Left = 10f },
                    Colour = colourProvider.Background4,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Left = 20f },
                    Spacing = new Vector2(15f, 0f),
                    Children = new[]
                    {
                        new BeatmapCountWedgeItem("HP Drain", "5.28"),
                        new BeatmapCountWedgeItem("Accuracy", "9.60"),
                        new BeatmapCountWedgeItem("Approach Rate", "6.78"),
                    },
                }
            };
        }
    }
}
