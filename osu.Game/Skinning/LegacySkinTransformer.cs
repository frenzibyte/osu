// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Legacy;
using static osu.Game.Skinning.SkinConfiguration;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Transformer used to handle support of legacy features for individual rulesets.
    /// </summary>
    public abstract class LegacySkinRulesetImplementation : ISkinRulesetImplementation
    {
        /// <summary>
        /// Whether the skin being transformed is able to provide legacy resources for the ruleset.
        /// </summary>
        public virtual bool IsProvidingLegacyResources => this.HasFont(LegacyFont.Combo);

        protected LegacySkinRulesetImplementation(ISkin skin)
            : base(skin)
        {
        }

        public virtual Drawable? GetDefaultDrawableComponent(ISkinComponentLookup lookup) => null;

        public virtual Drawable? GetDefaultRulesetLayout(SkinComponentsContainerLookup lookup) => null;

        public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            throw new System.NotImplementedException();
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (!(sampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacySample))
                return Skin.GetSample(sampleInfo);

            var playLayeredHitSounds = GetConfig<LegacySetting, bool>(LegacySetting.LayeredHitSounds);
            if (legacySample.IsLayered && playLayeredHitSounds?.Value == false)
                return new SampleVirtual();

            return base.GetSample(sampleInfo);
        }

        public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup) where TLookup : notnull where TValue : notnull
        {
            throw new System.NotImplementedException();
        }
    }
}
