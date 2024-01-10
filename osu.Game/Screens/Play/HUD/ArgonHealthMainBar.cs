// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthMainBar : ArgonHealthBar
    {
        private static readonly Colour4 colour = Colour4.White;
        public static readonly Colour4 GLOW_COLOUR = Color4Extensions.FromHex("#7ED7FD").Opacity(0.5f);

        public ArgonHealthMainBar()
        {
            Blending = BlendingParameters.Additive;
            BarColour = colour;
            GlowColour = GLOW_COLOUR;
            GlowPortion = 0.6f;
        }
    }
}
