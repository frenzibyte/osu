// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.IO.Stores;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class DefaultLegacySkin : LegacySkin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = Skinning.SkinInfo.CLASSIC_SKIN, // this is temporary until database storage is decided upon.
            Name = "osu! \"classic\" (2013)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(DefaultLegacySkin).GetInvariantInstantiationInfo()
        };

        public DefaultLegacySkin(Ruleset? ruleset, IStorageResourceProvider resources)
            : this(CreateInfo(), ruleset, resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public DefaultLegacySkin(SkinInfo skin, Ruleset? ruleset, IStorageResourceProvider resources)
            : base(skin, ruleset, resources, new NamespacedResourceStore<byte[]>(resources.Resources, "Skins/Legacy"))
        {
            Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);
            Configuration.CustomComboColours = new List<Color4>
            {
                new Color4(255, 192, 0, 255),
                new Color4(0, 202, 0, 255),
                new Color4(18, 124, 255, 255),
                new Color4(242, 24, 57, 255)
            };

            Configuration.ConfigDictionary[nameof(SkinConfiguration.LegacySetting.AllowSliderBallTint)] = @"true";

            Configuration.LegacyVersion = 2.7m;
        }
    }
}
