// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using osu.Framework.Input.Handlers;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests
{
    public class OsuDesktopTestBrowser : OsuTestBrowser
    {
        public override SettingsSubsection CreateSettingsSubsectionFor(InputHandler handler) => CreateDesktopSettingsSubsectionFor(handler) ?? base.CreateSettingsSubsectionFor(handler);
    }
}
