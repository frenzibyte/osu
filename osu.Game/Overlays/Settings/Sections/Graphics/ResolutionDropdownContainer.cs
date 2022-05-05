// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class ResolutionDropdownContainer : VisibilityContainer
    {
        private readonly BindableList<Size> resolutions = new BindableList<Size>(new[] { new Size(9999, 9999) });

        private ResolutionSettingsDropdown dropdown;
        private Bindable<Size> sizeFullscreen;

        private IBindable<Display> currentDisplay;
        private IBindable<WindowMode> windowMode;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);

            Child = dropdown = new ResolutionSettingsDropdown
            {
                LabelText = GraphicsSettingsStrings.Resolution,
                ShowsDefaultIndicator = false,
                ItemSource = resolutions,
                Current = sizeFullscreen
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            windowMode = host.Window.WindowMode.GetBoundCopy();
            windowMode.BindValueChanged(_ => updateResolutionsDropdown());

            currentDisplay = host.Window.CurrentDisplayBindable.GetBoundCopy();
            currentDisplay.BindValueChanged(_ => updateResolutionsDropdown(), true);
        }

        private void updateResolutionsDropdown()
        {
            if (resolutions.Count > 1 && host.Window.WindowMode.Value == WindowMode.Borderless)
                Show();
            else
                Hide();

            resolutions.RemoveRange(1, resolutions.Count - 1);

            if (currentDisplay.Value != null)
            {
                resolutions.AddRange(currentDisplay.Value.DisplayModes
                                                   .Where(m => m.Size.Width >= 800 && m.Size.Height >= 600)
                                                   .OrderByDescending(m => Math.Max(m.Size.Height, m.Size.Width))
                                                   .Select(m => m.Size)
                                                   .Distinct());
            }
        }

        protected override void PopIn() => Alpha = 1;
        protected override void PopOut() => Alpha = 0;

        private class ResolutionSettingsDropdown : SettingsDropdown<Size>
        {
            protected override OsuDropdown<Size> CreateDropdown() => new ResolutionDropdownControl();

            private class ResolutionDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Size item)
                {
                    if (item == new Size(9999, 9999))
                        return CommonStrings.Default;

                    return $"{item.Width}x{item.Height}";
                }
            }
        }
    }
}
