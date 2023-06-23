// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class PreviousUsernames : CompositeDrawable
    {
        private const int duration = 200;
        private const int width = 300;
        private const int move_offset = 15;
        private const int icon_size = 15;

        public readonly Bindable<APIUser?> User = new Bindable<APIUser?>();

        private readonly Container content;

        // private readonly TextFlowContainer text;
        private readonly Box background;

        private readonly TextFlowContainer textFlow;

        // private readonly SpriteText header;

        public PreviousUsernames()
        {
            AutoSizeAxes = Axes.Y;
            Width = width;

            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Alpha = 0f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding(10f),
                        Spacing = new Vector2(10f),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Size = new Vector2(icon_size),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Icon = FontAwesome.Solid.IdCard,
                                    },
                                    new HoverContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(icon_size + 20f),
                                        ActivateHover = showContent,
                                        BypassAutoSizeAxes = Axes.Both,
                                    }
                                }
                            },
                            textFlow = new TextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                AlwaysPresent = true,
                                Alpha = 0f,
                            },
                        }
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.GreySeaFoamDarker;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => content.ReceivePositionalInputAt(screenSpacePos);

        private void onUserChanged(ValueChangedEvent<APIUser?> user)
        {
            textFlow.Clear();

            string[]? usernames = user.NewValue?.PreviousUsernames;

            if (usernames == null || !usernames.Any())
                Hide();
            else
            {
                Show();

                textFlow.AddText(UsersStrings.ShowPreviousUsernames, cp => cp.Font = cp.Font.With(size: 10, weight: FontWeight.Regular));
                textFlow.NewLine();

                textFlow.AddText(string.Join(", ", usernames), cp => cp.Font = cp.Font.With(size: 12, weight: FontWeight.Bold));
            }
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hideContent();
        }

        private void showContent()
        {
            // text.FadeIn(duration, Easing.OutQuint);
            // header.FadeIn(duration, Easing.OutQuint);
            background.FadeIn(300f, Easing.OutQuint);
            textFlow.FadeIn(300f, Easing.OutQuint);
            content.MoveToY(-30f, 300f, Easing.OutQuint);
        }

        private void hideContent()
        {
            // text.FadeOut(300f, Easing.OutQuint);
            // header.FadeOut(300f, Easing.OutQuint);
            background.FadeOut(300f, Easing.OutQuint);
            textFlow.FadeOut(300f, Easing.OutQuint);
            content.MoveToY(0, 300f, Easing.OutQuint);
        }

        private partial class HoverContainer : Container
        {
            public Action? ActivateHover;

            protected override bool OnHover(HoverEvent e)
            {
                ActivateHover?.Invoke();
                return base.OnHover(e);
            }
        }
    }
}
