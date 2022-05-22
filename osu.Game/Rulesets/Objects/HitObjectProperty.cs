// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects
{
    public class HitObjectProperty<T>
    {
        private Bindable<T> backingBindable;

        /// <summary>
        /// A temporary field to store the current value to, if no bindable has been constructed for this property yet.
        /// </summary>
        private T backingValue;

        public Bindable<T> Bindable => backingBindable ??= new Bindable<T> { Value = backingValue };

        public T Value
        {
            get => backingBindable != null ? backingBindable.Value : backingValue;
            set
            {
                if (backingBindable != null)
                    backingBindable.Value = value;
                else
                    backingValue = value;
            }
        }
    }
}
