// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Extensions;

namespace osu.Game.Users.Drawables
{
    public class DrawableFlag : Sprite, IHasTooltip
    {
        [CanBeNull]
        private readonly string country;

        public LocalisableString TooltipText => country == null ? string.Empty : CountryExtensions.GetCountryName(country);

        public DrawableFlag(string country)
        {
            this.country = country;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            if (ts == null)
                throw new ArgumentNullException(nameof(ts));

            Texture = ts.Get($@"Flags/{country ?? "__"}") ?? ts.Get(@"Flags/__");
        }
    }
}
