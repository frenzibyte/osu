// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCountWedgeItem : CompositeDrawable
    {
        private readonly string label;
        private readonly string value;

        public BeatmapCountWedgeItem(string label, string value)
        {
            this.label = label;
            this.value = value;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 12 * 1.2f, weight: FontWeight.SemiBold),
                        Colour = colourProvider.Content2,
                        Text = label,
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 20 * 1.2f, weight: FontWeight.Regular),
                        Colour = colourProvider.Content1,
                        Text = value,
                    }
                }
            };
        }
    }
}
