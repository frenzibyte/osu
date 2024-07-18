// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Overlays
{
    public partial class OverlayColourProvider : Component
    {
        private readonly double providerDuration;
        private readonly Easing providerEasing;

        [Resolved]
        private OverlayColourPalette palette { get; set; } = null!;

        private readonly List<DrawableColourRegistration> registrations = new List<DrawableColourRegistration>();

        public OverlayColourProvider(double providerDuration = 300, Easing providerEasing = Easing.OutQuint)
        {
            this.providerDuration = providerDuration;
            this.providerEasing = providerEasing;
        }

        public void AssignColour(Drawable drawable, OverlayColourShade shade, Func<Colour4, Colour4>? adjustment = null)
        {
            var registration = new DrawableColourRegistration(new WeakReference<Drawable>(drawable), shade, adjustment);
            registrations.Add(registration);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            palette.Hue.BindValueChanged(_ => updateColours());
        }

        private void updateColours()
        {
            for (int i = 0; i < registrations.Count; i++)
            {
                var registration = registrations[i];

                if (!registration.Drawable.TryGetTarget(out var drawable))
                {
                    registrations.RemoveAt(i--);
                    continue;
                }

                var colour = palette.Get(registration.Shade);

                if (registration.Adjustment != null)
                    colour = registration.Adjustment(colour);

                drawable.FadeColour(colour, providerDuration, providerEasing);
            }
        }

        private record DrawableColourRegistration(WeakReference<Drawable> Drawable, OverlayColourShade Shade, Func<Colour4, Colour4>? Adjustment);
    }

    public static class OverlayColourProviderExtensions
    {
        public static T WithOverlayColour<T>(this T drawable, OverlayColourProvider colourProvider, OverlayColourShade shade, Func<Colour4, Colour4>? adjustment = null)
            where T : Drawable
        {
            colourProvider.AssignColour(drawable, shade, adjustment);
            return drawable;
        }
    }
}
