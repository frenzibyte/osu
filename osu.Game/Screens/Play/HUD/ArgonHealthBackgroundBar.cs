// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthBackgroundBar : ArgonHealthBar
    {
        // todo: try to use existing properties in ArgonHealthBar.
        protected override Color4 ColourAt(float position)
        {
            if (position <= 0.16f)
                return Color4.White.Opacity(0.8f);

            return Interpolation.ValueAt(position,
                Color4.White.Opacity(0.8f),
                Color4.Black.Opacity(0.2f),
                -0.5f, 1f, Easing.OutQuint);
        }
    }
}
