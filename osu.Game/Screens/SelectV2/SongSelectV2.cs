// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.SelectV2.Footer;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    public partial class SongSelectV2 : ScreenWithBeatmapBackground
    {
        private const float logo_scale = 0.4f;

        protected const float BACKGROUND_BLUR = 20;

        private BeatmapInfoWedgeV2 beatmapInfoWedge = null!;

        private readonly ModSelectOverlay modSelectOverlay = new SoloModSelectOverlay();

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public override bool ShowFooter => true;

        [Resolved]
        private OsuLogo? logo { get; set; }

        private Bindable<bool> configBackgroundBlur = null!;

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new ScreenFooterButtonMods(modSelectOverlay) { Current = Mods },
            new ScreenFooterButtonRandom(),
            new ScreenFooterButtonOptions(),
        };

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            configBackgroundBlur = config.GetBindable<bool>(OsuSetting.SongSelectBackgroundBlur);
            configBackgroundBlur.BindValueChanged(_ =>
            {
                if (this.IsCurrentScreen())
                    updateScreenBackground();
            });

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black, Color4.Black.Opacity(0f)),
                    Height = (float)Math.Sqrt(0.5f),
                },
                beatmapInfoWedge = new BeatmapInfoWedgeV2(),
                modSelectOverlay,
            });
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();

            Beatmap.BindValueChanged(onBeatmapChanged, true);

            modSelectOverlay.State.BindValueChanged(onModSelectStateChanged, true);
            modSelectOverlay.SelectedMods.BindTo(Mods);

            beatmapInfoWedge.Show();

            updateScreenBackground();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn();

            beatmapInfoWedge.Show();

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            updateScreenBackground();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(400, Easing.OutQuint);

            beatmapInfoWedge.Hide();

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(400, Easing.OutQuint);

            beatmapInfoWedge.Hide();

            return base.OnExiting(e);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (logo.Alpha > 0.8f)
                Footer?.StartTrackingLogo(logo, 400, Easing.OutQuint);
            else
            {
                logo.Hide();
                logo.ScaleTo(0.2f);
                Footer?.StartTrackingLogo(logo);
            }

            logo.FadeIn(240, Easing.OutQuint);
            logo.ScaleTo(logo_scale, 240, Easing.OutQuint);

            logo.Action = () =>
            {
                this.Push(new PlayerLoaderV2(() => new SoloPlayer()));
                return false;
            };
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            Footer?.StopTrackingLogo();
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            Scheduler.AddDelayed(() => Footer?.StopTrackingLogo(), 120);
            logo.ScaleTo(0.2f, 120, Easing.Out);
            logo.FadeOut(120, Easing.Out);
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> b)
        {
            if (this.IsCurrentScreen())
                updateScreenBackground();

            beatmapInfoWedge.Beatmap = b.NewValue;
        }

        private void updateScreenBackground()
        {
            ApplyToBackground(backgroundModeBeatmap =>
            {
                backgroundModeBeatmap.Beatmap = Beatmap.Value;
                backgroundModeBeatmap.BlurAmount.Value = configBackgroundBlur.Value ? BACKGROUND_BLUR : 0f;
                backgroundModeBeatmap.DimWhenUserSettingsIgnored.Value = configBackgroundBlur.Value ? 0 : 0.4f;
                backgroundModeBeatmap.IgnoreUserSettings.Value = true;
                backgroundModeBeatmap.FadeColour(Color4.White, 250);
            });
        }

        private void onModSelectStateChanged(ValueChangedEvent<Visibility> v)
        {
            if (v.NewValue == Visibility.Visible)
                logo?.ScaleTo(0f, 400, Easing.OutQuint).FadeTo(0f, 200, Easing.OutQuint);
            else
                logo?.ScaleTo(logo_scale, 400, Easing.OutQuint).FadeTo(1f, 200, Easing.OutQuint);
        }

        private partial class SoloModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;
        }

        private partial class PlayerLoaderV2 : PlayerLoader
        {
            public override bool ShowFooter => true;

            public PlayerLoaderV2(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }
    }
}
