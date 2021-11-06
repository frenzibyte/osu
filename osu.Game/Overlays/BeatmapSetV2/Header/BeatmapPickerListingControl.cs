// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapPickerListingControl : CompositeDrawable, IHasPopover
    {
        private readonly BindableWithCurrent<APIBeatmap> current = new BindableWithCurrent<APIBeatmap>();

        public Bindable<APIBeatmap> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private Box hover;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<APIBeatmap>> availableBeatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(20f);
            Padding = new MarginPadding(-4f);

            // todo: needs some more fixing, button sucks ass basically.
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(4f),
                    Children = new[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Size = new Vector2(9f),
                            Icon = OsuIcon.RulesetOsu,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Size = new Vector2(9f),
                            Icon = OsuIcon.RulesetOsu,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Size = new Vector2(9f),
                            Icon = OsuIcon.RulesetOsu,
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Size = new Vector2(9f),
                            Icon = OsuIcon.RulesetOsu,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5f,
                    Child = hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3.Opacity(0f),
                        Blending = BlendingParameters.Additive,
                    },
                },
                new HoverClickSounds(HoverSampleSet.Button),
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateHoverState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e) => updateHoverState();

        private Visibility popoverState;

        protected override bool OnClick(ClickEvent e)
        {
            // todo: this is absolutely fucked, needs framework-side support for robust determination.
            switch (popoverState)
            {
                case Visibility.Hidden:
                    hover.FlashColour(colourProvider.Background3.Opacity(0.7f), 800, Easing.OutQuint);

                    this.ShowPopover();
                    popoverState = Visibility.Visible;
                    break;

                case Visibility.Visible:
                    this.HidePopover();
                    popoverState = Visibility.Hidden;

                    updateHoverState();
                    break;
            }

            return true;
        }

        private void updateHoverState()
        {
            if (popoverState == Visibility.Visible || IsHovered)
                hover.FadeColour(colourProvider.Background3.Opacity(0.3f), 500, Easing.OutQuint);
            else
                hover.FadeColour(colourProvider.Background3.Opacity(0f), 500, Easing.OutQuint);
        }

        public Popover GetPopover() => new BeatmapPickerListPopover(availableBeatmaps.Value);
    }
}
