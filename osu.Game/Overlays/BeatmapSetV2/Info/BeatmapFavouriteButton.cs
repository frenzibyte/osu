// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    // todo: this really needs rethinking it, looks like an absolute piece of shit.
    public class BeatmapFavouriteButton : LoadingButton, IHasCurrentValue<APIBeatmapSet>
    {
        private RoundedButton button;
        private FillFlowContainer buttonContent;

        private SpriteIcon favouritesIcon;
        private OsuSpriteText favouritesCount;

        private readonly BindableBool favourited = new BindableBool();

        private readonly BindableWithCurrent<APIBeatmapSet> current = new BindableWithCurrent<APIBeatmapSet>();

        public Bindable<APIBeatmapSet> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        public override LocalisableString TooltipText
        {
            get
            {
                if (!Enabled.Value)
                    return string.Empty;

                return favourited.Value ? BeatmapsetsStrings.ShowDetailsUnfavourite : BeatmapsetsStrings.ShowDetailsFavourite;
            }
        }

        public BeatmapFavouriteButton()
        {
            AutoSizeAxes = Axes.Both;
            Action = postFavourite;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            favourited.BindValueChanged(f =>
            {
                int count = Current.Value?.FavouriteCount ?? 0;

                if (f.NewValue)
                {
                    if (Current.Value?.HasFavourited == false)
                        count++;

                    favouritesIcon.Icon = FontAwesome.Solid.Heart;
                }
                else
                {
                    if (Current.Value?.HasFavourited == true)
                        count--;

                    favouritesIcon.Icon = FontAwesome.Regular.Heart;
                }

                favouritesCount.Alpha = count > 0 ? 1 : 0;
                favouritesCount.Text = count.ToString("N0");
            }, true);

            Current.BindValueChanged(set =>
            {
                favourited.Value = set.NewValue?.HasFavourited ?? false;
                Enabled.Value = set.NewValue != null;
            }, true);
        }

        private void postFavourite()
        {
            IsLoading = true;

            var action = favourited.Value ? BeatmapFavouriteAction.UnFavourite : BeatmapFavouriteAction.Favourite;
            var request = new PostBeatmapFavouriteRequest(Current.Value.OnlineID, action);

            request.Success += () =>
            {
                IsLoading = false;
                favourited.Toggle();
            };

            api.Queue(request);
        }

        protected override void OnLoadStarted() => buttonContent.FadeOut(200, Easing.OutQuint);
        protected override void OnLoadFinished() => buttonContent.FadeIn(200, Easing.OutQuint);

        protected override Drawable CreateContent()
        {
            button = new RoundedButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // todo: this will cause the button to look as if it has a right-centered origin, due to the content scaling affecting buttonContent size, which then affect the overall button size (very weird stuff).
                AutoSizeAxes = Axes.X,
                Height = 30f,
                BackgroundColour = OverlayColourProvider.Pink.Colour3,
                Action = () => Action?.Invoke(),
                Enabled = { BindTarget = Enabled },
            };

            button.Add(buttonContent = new FillFlowContainer
            {
                AlwaysPresent = true,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3f, 0f),
                Margin = new MarginPadding { Horizontal = 10 },
                Children = new Drawable[]
                {
                    favouritesIcon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(10f),
                    },
                    favouritesCount = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                        Padding = new MarginPadding { Bottom = 2f },
                    },
                }
            });

            return button;
        }
    }
}
