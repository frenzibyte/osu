// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedgeStatistic : CompositeDrawable, IHasTooltip
    {
        private readonly IconUsage icon;
        private readonly LocalisableString value;

        public LocalisableString TooltipText { get; }

        public BeatmapMainWedgeStatistic(IconUsage icon, LocalisableString value, LocalisableString tooltip)
        {
            this.icon = icon;
            this.value = value;

            TooltipText = tooltip;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 8; // todo: ?
            Size = new Vector2(120, 30);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.25f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4f, 0f),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = icon,
                            Size = new Vector2(20f),
                            Colour = colourProvider.Content2,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = value,
                            Font = OsuFont.Torus.With(size: 19.2f, weight: FontWeight.SemiBold),
                            Colour = colourProvider.Content2,
                            UseFullGlyphHeight = false,
                        },
                    },
                }
            };
        }
    }
}
