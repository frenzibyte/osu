// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Objects
{
    public class HitObjectListProperty<T>
    {
        private BindableList<T> backingBindable;

        /// <summary>
        /// A temporary field to store the current list to, if no bindable has been constructed for this property yet.
        /// </summary>
        private readonly List<T> backingList = new List<T>();

        public BindableList<T> Bindable => backingBindable ??= new BindableList<T>(backingList);

        public IList<T> List
        {
            get => backingBindable ?? (IList<T>)backingList;
            set
            {
                if (backingBindable != null)
                {
                    backingBindable.Clear();
                    backingBindable.AddRange(value);
                }
                else
                {
                    backingList.Clear();
                    backingList.AddRange(value);
                }
            }
        }
    }
}
