// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public interface ISettingsDropdown
    {
        event Action<MenuState> MenuStateChanged;

        MenuState State { get; }
    }
}
