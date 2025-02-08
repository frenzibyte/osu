// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Carousel;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class UpdateBeatmapSetButtonV2 : CompositeDrawable, IHasTooltip
    {
        public LocalisableString TooltipText => buttonState.Value == ButtonState.UpdateAvailable
            ? "Update beatmap with online changes"
            : string.Empty;

        private BeatmapSetInfo? beatmapSet;

        public BeatmapSetInfo? BeatmapSet
        {
            get => beatmapSet;
            set
            {
                beatmapSet = value;

                if (IsLoaded)
                    beatmapChanged();
            }
        }

        private Container content = null!;
        private Sprite buttonSprite = null!;
        private Drawable hoverLayer = null!;
        private SpriteIcon icon = null!;

        private ArchiveDownloadRequest<IBeatmapSetInfo>? currentDownloadRequest;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private LoginOverlay? loginOverlay { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        private Bindable<bool> preferNoVideo = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            => content.ReceivePositionalInputAt(screenSpacePos);

        private readonly Bindable<ButtonState> buttonState = new Bindable<ButtonState>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, TextureStore textures)
        {
            preferNoVideo = config.GetBindable<bool>(OsuSetting.PreferNoVideo);

            InternalChild = content = new Container
            {
                Size = new Vector2(80f),
                Children = new[]
                {
                    buttonSprite = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(110f),
                        Texture = textures.Get(@"Select/update-button"),
                        Blending = BlendingParameters.Additive,
                    },
                    hoverLayer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Blending = BlendingParameters.Additive,
                        Size = new Vector2(80f),
                        CornerRadius = 10f,
                        Masking = true,
                        // EdgeEffect = new EdgeEffectParameters
                        // {
                        //     Type = EdgeEffectType.Glow,
                        //     Colour = colours.Orange1,
                        //     Radius = 10f,
                        // },
                        Child = new Box { RelativeSizeAxes = Axes.Both },
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.Centre,
                        X = 16f,
                        Icon = FontAwesome.Solid.SyncAlt,
                        Size = new Vector2(16f),
                    },
                    new HoverClickSounds(HoverSampleSet.Button),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapChanged();

            buttonState.BindValueChanged(_ => updateDisplay());
            updateDisplay();
        }

        private void beatmapChanged()
        {
            // buttonState.Value = ...
            // alpha = ...
            updateDisplay();
        }

        private void updateDisplay()
        {
            // content.MoveToX(IsHovered ? -20f : 0f, 300, Easing.OutQuint);
            // icon.MoveToX(IsHovered ? 28f : 16f, 300, Easing.OutQuint);

            switch (buttonState.Value)
            {
                case ButtonState.UpdateAvailable:
                    buttonSprite.FadeColour(colours.Orange2, 300, Easing.OutQuint);
                    hoverLayer.FadeColour(colours.Orange2, 300, Easing.OutQuint);
                    hoverLayer.FadeTo(IsHovered ? 0.3f : 0f, 300, Easing.OutQuint);

                    icon.Icon = FontAwesome.Solid.SyncAlt;
                    icon.ClearTransforms();
                    icon.Spin(4000, RotationDirection.Clockwise, icon.Rotation);

                    content.MoveToX(0f, 300, Easing.OutQuint);
                    this.FadeIn(300, Easing.OutQuint);
                    break;

                case ButtonState.Updating:
                    buttonSprite.FadeColour(colours.Blue1, 300, Easing.OutQuint);
                    hoverLayer.FadeColour(colours.Blue1, 300, Easing.OutQuint);
                    hoverLayer.FadeTo(IsHovered ? 0.5f : 0.3f, 300)
                              .Then()
                              .FadeTo(IsHovered ? 0.5f : 0.1f, 300)
                              .Loop();

                    icon.Icon = FontAwesome.Solid.SyncAlt;
                    icon.Spin(4000, RotationDirection.Clockwise, icon.Rotation);

                    content.MoveToX(0f, 300, Easing.OutQuint);
                    this.FadeIn(300, Easing.OutQuint);
                    break;

                case ButtonState.Updated:
                    buttonSprite.FadeColour(colours.Green1, 300, Easing.OutQuint);
                    hoverLayer.FadeColour(colours.Green1, 300, Easing.OutQuint);
                    hoverLayer.FadeTo(0.2f, 300, Easing.OutQuint);

                    icon.Icon = FontAwesome.Regular.CheckCircle;
                    icon.ClearTransforms();
                    icon.Rotation = 0;

                    content.Delay(500).MoveToX(20f, 500, Easing.OutQuint);
                    this.Delay(500).FadeOut(200, Easing.OutQuint);
                    break;
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (buttonState.Value == ButtonState.UpdateAvailable)
                performUpdate();

            hoverLayer.FadeTo(1f)
                      .FadeTo(0.3f, 500, Easing.OutQuint);
            return true;
        }

        private bool updateConfirmed;

        private void performUpdate()
        {
            Debug.Assert(beatmapSet != null);

            if (!api.IsLoggedIn)
            {
                loginOverlay?.Show();
                return;
            }

            if (dialogOverlay != null && beatmapSet.Status == BeatmapOnlineStatus.LocallyModified && !updateConfirmed)
            {
                dialogOverlay.Push(new UpdateLocalConfirmationDialog(() =>
                {
                    updateConfirmed = true;
                    performUpdate();
                }));

                return;
            }

            updateConfirmed = false;

            beatmapDownloader.DownloadAsUpdate(beatmapSet, preferNoVideo.Value);
            attachExistingDownload();
        }

        private void attachExistingDownload()
        {
            Debug.Assert(beatmapSet != null);
            currentDownloadRequest = beatmapDownloader.GetExistingDownload(beatmapSet);

            if (currentDownloadRequest != null)
            {
                buttonState.Value = ButtonState.Updating;
                Scheduler.AddDelayed(() => buttonState.Value = ButtonState.Updated, 2000);
                // currentDownloadRequest.Failure += _ => buttonState.Value = ButtonState.UpdateAvailable;
            }
            else
            {
                buttonState.Value = ButtonState.UpdateAvailable;
            }
        }

        public enum ButtonState
        {
            UpdateAvailable,
            Updating,
            Updated,
        }
    }
}
