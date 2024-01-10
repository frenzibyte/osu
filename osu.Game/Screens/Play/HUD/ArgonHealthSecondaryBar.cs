// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthSecondaryBar : ArgonHealthBar
    {
        public ArgonHealthSecondaryBar()
        {
            Blending = BlendingParameters.Additive;
            Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0.8f), Color4.White);
            BarColour = Color4.White;
            GlowColour = ArgonHealthMainBar.GLOW_COLOUR;
            PathRadius = 40f;
            // Kinda hacky, but results in correct positioning with increased path radius.
            Margin = new MarginPadding(-30f);
            GlowPortion = 0.9f;
        }
    }
}
