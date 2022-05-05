// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class ScalingSettingsContainer : VisibilityContainer
    {
        private const int transition_duration = 400;

        private FillFlowContainer<SettingsSlider<float>> settingsFlow;

        public IEnumerable<SettingsSlider<float>> Settings => settingsFlow.Children;

        private Bindable<ScalingMode> scalingMode;
        private Bindable<float> scalingPositionX;
        private Bindable<float> scalingPositionY;
        private Bindable<float> scalingSizeX;
        private Bindable<float> scalingSizeY;

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeDuration = transition_duration;
            AutoSizeEasing = Easing.OutQuint;

            Masking = true;

            scalingMode = config.GetBindable<ScalingMode>(OsuSetting.Scaling);
            scalingSizeX = config.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = config.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = config.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = config.GetBindable<float>(OsuSetting.ScalingPositionY);

            Child = settingsFlow = new FillFlowContainer<SettingsSlider<float>>
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                Children = new[]
                {
                    new SettingsSlider<float>
                    {
                        LabelText = GraphicsSettingsStrings.HorizontalPosition,
                        Current = scalingPositionX,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true
                    },
                    new SettingsSlider<float>
                    {
                        LabelText = GraphicsSettingsStrings.VerticalPosition,
                        Current = scalingPositionY,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true
                    },
                    new SettingsSlider<float>
                    {
                        LabelText = GraphicsSettingsStrings.HorizontalScale,
                        Current = scalingSizeX,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true
                    },
                    new SettingsSlider<float>
                    {
                        LabelText = GraphicsSettingsStrings.VerticalScale,
                        Current = scalingSizeY,
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            settingsFlow.ForEach(bindPreviewEvent);

            scalingMode.BindValueChanged(s =>
            {
                State.Value = s.NewValue == ScalingMode.Off ? Visibility.Hidden : Visibility.Visible;

                foreach (var setting in settingsFlow)
                    setting.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything;
            }, true);
        }

        private Drawable preview;

        private void bindPreviewEvent(SettingsSlider<float> slider)
        {
            slider.Current.ValueChanged += _ =>
            {
                switch (scalingMode.Value)
                {
                    case ScalingMode.Gameplay:
                        showPreview();
                        break;
                }
            };
        }

        private void showPreview()
        {
            if (preview?.IsAlive != true)
                game.Add(preview = new ScalingPreview());

            preview.FadeOutFromOne(1500);
            preview.Expire();
        }

        protected override void PopIn() => settingsFlow.AutoSizeAxes = Axes.Y;

        protected override void PopOut()
        {
            settingsFlow.AutoSizeAxes = Axes.None;
            settingsFlow.Height = 0;
        }

        private class ScalingPreview : ScalingContainer
        {
            public ScalingPreview()
            {
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                };
            }
        }
    }
}
