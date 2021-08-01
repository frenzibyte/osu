// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers
{
    public class OverlayVisibilityBindable : Bindable<Visibility>
    {
        public readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>();

        public override Visibility Value
        {
            get => base.Value;
            set
            {
                if (OverlayActivationMode.Value == OverlayActivation.Disabled)
                {
                    // todo: visual/audible feedback that this operation could not complete.
                    return;
                }

                base.Value = value;
            }
        }

        public OverlayVisibilityBindable()
        {
            OverlayActivationMode.BindValueChanged(a =>
            {
                if (a.NewValue == OverlayActivation.Disabled)
                    base.Value = Visibility.Hidden;
            }, true);
        }
    }
}
