// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayColourPalette
    {
        /// <summary>
        /// The hue degree associated with the colour shades provided by this <see cref="OverlayColourPalette"/>.
        /// </summary>
        public readonly BindableInt Hue;

        public OverlayColourPalette(OverlayColourScheme colourScheme)
            : this(colourScheme.GetHue())
        {
        }

        public OverlayColourPalette(int hue)
        {
            Hue = new BindableInt(hue);
        }

        public Colour4 Get(OverlayColourShade shade)
        {
            switch (shade)
            {
                // Note that the following five colours are also defined in `OsuColour` as `{colourScheme}{0,1,2,3,4}`.
                // The difference as to which should be used where comes down to context.
                // If the colour in question is supposed to always match the view in which it is displayed theme-wise, use `OverlayColourProvider`.
                case OverlayColourShade.Colour0:
                    return getColour(1, 0.8f);

                case OverlayColourShade.Colour1:
                    return getColour(1, 0.7f);

                case OverlayColourShade.Colour2:
                    return getColour(0.8f, 0.6f);

                case OverlayColourShade.Colour3:
                    return getColour(0.6f, 0.5f);

                case OverlayColourShade.Colour4:
                    return getColour(0.4f, 0.3f);

                case OverlayColourShade.Highlight1:
                    return getColour(1, 0.7f);

                case OverlayColourShade.Content1:
                    return getColour(0.4f, 1);

                case OverlayColourShade.Content2:
                    return getColour(0.4f, 0.9f);

                case OverlayColourShade.Light1:
                    return getColour(0.4f, 0.8f);

                case OverlayColourShade.Light2:
                    return getColour(0.4f, 0.75f);

                case OverlayColourShade.Light3:
                    return getColour(0.4f, 0.7f);

                case OverlayColourShade.Light4:
                    return getColour(0.4f, 0.5f);

                case OverlayColourShade.Dark1:
                    return getColour(0.2f, 0.35f);

                case OverlayColourShade.Dark2:
                    return getColour(0.2f, 0.3f);

                case OverlayColourShade.Dark3:
                    return getColour(0.2f, 0.25f);

                case OverlayColourShade.Dark4:
                    return getColour(0.2f, 0.2f);

                case OverlayColourShade.Dark5:
                    return getColour(0.2f, 0.15f);

                case OverlayColourShade.Dark6:
                    return getColour(0.2f, 0.1f);

                case OverlayColourShade.Foreground1:
                    return getColour(0.1f, 0.6f);

                case OverlayColourShade.Background1:
                    return getColour(0.1f, 0.4f);

                case OverlayColourShade.Background2:
                    return getColour(0.1f, 0.3f);

                case OverlayColourShade.Background3:
                    return getColour(0.1f, 0.25f);

                case OverlayColourShade.Background4:
                    return getColour(0.1f, 0.2f);

                case OverlayColourShade.Background5:
                    return getColour(0.1f, 0.15f);

                case OverlayColourShade.Background6:
                    return getColour(0.1f, 0.1f);

                default:
                    throw new ArgumentOutOfRangeException(nameof(shade));
            }
        }

        private Color4 getColour(float saturation, float lightness) => Framework.Graphics.Colour4.FromHSL(Hue.Value / 360f, saturation, lightness);
    }
}
