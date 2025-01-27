// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence label.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCountWedge : CompositeDrawable
    {
        /// Todo: move this const out to song select when more new design elements are implemented for the beatmap details area, since it applies to text alignment of various elements
        private const float text_margin = 62;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        public BeatmapCountWedge()
        {
            Y = 320f;
            Width = 350f;
            Height = 50f;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10f;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = shear,
                    CornerRadius = 10f,
                    Masking = true,
                    Margin = new MarginPadding { Left = -10f },
                    Colour = colourProvider.Background4,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Left = text_margin },
                    Spacing = new Vector2(15f, 0f),
                    Children = new[]
                    {
                        new BeatmapCountWedgeItem("Circle Count", "325"),
                        new BeatmapCountWedgeItem("Slider Count", "140"),
                        new BeatmapCountWedgeItem("Spinner Count", "4"),
                    },
                }
            };
        }
    }
}
