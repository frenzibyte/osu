// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlayerSettingsGroup : SettingsToolboxGroup
    {
        private const int container_width = 270;

        public PlayerSettingsGroup(string title)
            : base(title)
        {
            Width = container_width;
            AutoSizeAxes = Axes.Y;
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);

            // Importantly, return true to correctly take focus away from PlayerLoader.
            return true;
        }
    }
}
