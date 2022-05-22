// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects
{
    public class IndirectBindable<T> : Bindable<T>
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;

        public IndirectBindable(Func<T> getter, Action<T> setter, T defaultValue = default)
        {
            this.getter = getter;
            this.setter = setter;

            InternalValue = Default = defaultValue;
        }

        protected override T InternalValue { get => getter(); set => setter(value); }
    }
}
