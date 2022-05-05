// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.LayoutHeader;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();
        private readonly IBindableList<WindowMode> windowModes = new BindableList<WindowMode>();

        private SettingsDropdown<Display> displayDropdown;
        private SettingsDropdown<WindowMode> windowModeDropdown;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            if (host.Window != null)
            {
                currentDisplay.BindTo(host.Window.CurrentDisplayBindable);
                windowModes.BindTo(host.Window.SupportedWindowModes);
            }

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsDropdown<WindowMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenMode,
                    ItemSource = windowModes,
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                displayDropdown = new DisplaySettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Display,
                    Items = host.Window?.Displays,
                    Current = currentDisplay,
                },
                new ResolutionDropdownContainer(),
                new SettingsSlider<float, UIScaleSlider>
                {
                    LabelText = GraphicsSettingsStrings.UIScaling,
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                    Keywords = new[] { "scale", "letterbox" },
                },
                new SettingsEnumDropdown<ScalingMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenScaling,
                    Current = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                    Keywords = new[] { "scale", "letterbox" },
                },
                new ScalingSettingsContainer(),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            windowModeDropdown.Current.BindValueChanged(mode =>
            {
                updateDisplayDropdowns();

                windowModeDropdown.WarningText = mode.NewValue != WindowMode.Fullscreen ? GraphicsSettingsStrings.NotFullscreenNote : default;
            }, true);

            windowModes.BindCollectionChanged((sender, args) =>
            {
                if (windowModes.Count > 1)
                    windowModeDropdown.Show();
                else
                    windowModeDropdown.Hide();
            }, true);

            currentDisplay.BindValueChanged(display => Scheduler.AddOnce(updateDisplayDropdowns), true);

            void updateDisplayDropdowns()
            {
                if (displayDropdown.Items.Count() > 1)
                    displayDropdown.Show();
                else
                    displayDropdown.Hide();
            }
        }

        private class UIScaleSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }

        private class DisplaySettingsDropdown : SettingsDropdown<Display>
        {
            protected override OsuDropdown<Display> CreateDropdown() => new DisplaySettingsDropdownControl();

            private class DisplaySettingsDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Display item)
                {
                    return $"{item.Index}: {item.Name} ({item.Bounds.Width}x{item.Bounds.Height})";
                }
            }
        }
    }
}
