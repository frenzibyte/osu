// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class TagButton : CompositeDrawable
    {
        private readonly string tag;

        private Box box = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public Action? Action;

        public TagButton(string tag)
        {
            this.tag = tag;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 1.5f;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = colourProvider.Light1,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = tag,
                    Colour = colourProvider.Background4,
                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Horizontal = 3f },
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            box.FadeColour(colourProvider.Content2, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            box.FadeColour(colourProvider.Light1, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            box.FlashColour(colourProvider.Content1, 300, Easing.OutQuint);
            Action?.Invoke();
            return true;
        }
    }
}
