// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class BarLinePlayfield : ScrollingPlayfield
    {
        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RegisterPool<BarLine, DrawableBarLine>(15);
        }
    }
}
