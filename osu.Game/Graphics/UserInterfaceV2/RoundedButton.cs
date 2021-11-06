// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class RoundedButton : OsuButton, IFilterable
    {
        protected override Color4 DefaultBackgroundColour => ColourProvider?.Highlight1 ?? Colours.Blue3;

        public override float Height
        {
            get => base.Height;
            set
            {
                base.Height = value;

                if (IsLoaded)
                    updateCornerRadius();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCornerRadius();
        }

        private void updateCornerRadius() => Content.CornerRadius = DrawHeight / 2;

        public virtual IEnumerable<string> FilterTerms => new[] { Text.ToString() };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        protected override SpriteText CreateText() => base.CreateText().With(s =>
        {
            s.Font = s.Font.With(weight: FontWeight.SemiBold);
        });
    }
}
