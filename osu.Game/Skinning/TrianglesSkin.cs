﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps.Formats;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning.Triangles;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class TrianglesSkin : Skin
    {
        public static SkinInfo CreateInfo() => new SkinInfo
        {
            ID = osu.Game.Skinning.SkinInfo.TRIANGLES_SKIN,
            Name = "osu! \"triangles\" (2017)",
            Creator = "team osu!",
            Protected = true,
            InstantiationInfo = typeof(TrianglesSkin).GetInvariantInstantiationInfo()
        };

        private readonly IStorageResourceProvider? resources;

        public TrianglesSkin(Ruleset? ruleset, IStorageResourceProvider? resources)
            : this(CreateInfo(), ruleset, resources)
        {
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        public TrianglesSkin(SkinInfo skin, Ruleset? ruleset, IStorageResourceProvider? resources)
            : base(skin, ruleset, resources)
        {
            this.resources = resources;
        }

        protected override Texture? GetTextureImplementation(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => Textures?.Get(componentName, wrapModeS, wrapModeT);

        protected override ISample? GetSampleImplementation(ISampleInfo sampleInfo)
        {
            foreach (string lookup in sampleInfo.LookupNames)
            {
                var sample = Samples?.Get(lookup) ?? resources?.AudioManager?.Samples.Get(lookup);
                if (sample != null)
                    return sample;
            }

            return null;
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            // Temporary until default skin has a valid hit lighting.
            if ((lookup as SkinnableSprite.SpriteComponentLookup)?.LookupName == @"lighting") return Drawable.Empty();

            switch (lookup)
            {
                case SkinComponentsContainerLookup containerLookup:
                    if (base.GetDrawableComponent(lookup) is UserConfiguredLayoutContainer c)
                        return c;

                    // Only handle global level defaults for now.
                    if (containerLookup.Ruleset != null)
                        return null;

                    return null;
            }

            return base.GetDrawableComponent(lookup);
        }

        protected override Drawable? GetDefaultGlobalLayout(SkinComponentsContainerLookup.TargetArea area)
        {
            switch (area)
            {
                case SkinComponentsContainerLookup.TargetArea.SongSelect:
                    var songSelectComponents = new DefaultSkinComponentsContainer(_ =>
                    {
                        // do stuff when we need to.
                    });

                    return songSelectComponents;

                case SkinComponentsContainerLookup.TargetArea.MainHUDComponents:
                    var skinnableTargetWrapper = new DefaultSkinComponentsContainer(container =>
                    {
                        var score = container.OfType<DefaultScoreCounter>().FirstOrDefault();
                        var accuracy = container.OfType<DefaultAccuracyCounter>().FirstOrDefault();
                        var combo = container.OfType<DefaultComboCounter>().FirstOrDefault();
                        var ppCounter = container.OfType<PerformancePointsCounter>().FirstOrDefault();
                        var songProgress = container.OfType<DefaultSongProgress>().FirstOrDefault();
                        var keyCounter = container.OfType<DefaultKeyCounterDisplay>().FirstOrDefault();

                        if (score != null)
                        {
                            score.Anchor = Anchor.TopCentre;
                            score.Origin = Anchor.TopCentre;

                            // elements default to beneath the health bar
                            const float vertical_offset = 30;

                            const float horizontal_padding = 20;

                            score.Position = new Vector2(0, vertical_offset);

                            if (ppCounter != null)
                            {
                                ppCounter.Y = score.Position.Y + ppCounter.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).Y - 4;
                                ppCounter.Origin = Anchor.TopCentre;
                                ppCounter.Anchor = Anchor.TopCentre;
                            }

                            if (accuracy != null)
                            {
                                accuracy.Position = new Vector2(-accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 - horizontal_padding, vertical_offset + 5);
                                accuracy.Origin = Anchor.TopRight;
                                accuracy.Anchor = Anchor.TopCentre;

                                if (combo != null)
                                {
                                    combo.Position = new Vector2(accuracy.ScreenSpaceDeltaToParentSpace(score.ScreenSpaceDrawQuad.Size).X / 2 + horizontal_padding, vertical_offset + 5);
                                    combo.Anchor = Anchor.TopCentre;
                                }
                            }

                            var hitError = container.OfType<HitErrorMeter>().FirstOrDefault();

                            if (hitError != null)
                            {
                                hitError.Anchor = Anchor.CentreLeft;
                                hitError.Origin = Anchor.CentreLeft;
                            }

                            var hitError2 = container.OfType<HitErrorMeter>().LastOrDefault();

                            if (hitError2 != null)
                            {
                                hitError2.Anchor = Anchor.CentreRight;
                                hitError2.Scale = new Vector2(-1, 1);
                                // origin flipped to match scale above.
                                hitError2.Origin = Anchor.CentreLeft;
                            }
                        }

                        if (songProgress != null && keyCounter != null)
                        {
                            const float padding = 10;

                            // Hard to find this at runtime, so taken from the most expanded state during replay.
                            const float song_progress_offset_height = 73;

                            keyCounter.Anchor = Anchor.BottomRight;
                            keyCounter.Origin = Anchor.BottomRight;
                            keyCounter.Position = new Vector2(-padding, -(song_progress_offset_height + padding));
                        }
                    })
                    {
                        Children = new Drawable[]
                        {
                            new DefaultComboCounter(),
                            new DefaultScoreCounter(),
                            new DefaultAccuracyCounter(),
                            new DefaultHealthDisplay(),
                            new DefaultSongProgress(),
                            new DefaultKeyCounterDisplay(),
                            new BarHitErrorMeter(),
                            new BarHitErrorMeter(),
                            new TrianglesPerformancePointsCounter()
                        }
                    };

                    return skinnableTargetWrapper;
            }

            return null;
        }

        protected override IBindable<TValue>? GetConfigImplementation<TLookup, TValue>(TLookup lookup)
        {
            // todo: this code is pulled from LegacySkin and should not exist.
            // will likely change based on how databased storage of skin configuration goes.
            switch (lookup)
            {
                case GlobalSkinColours global:
                    switch (global)
                    {
                        case GlobalSkinColours.ComboColours:
                        {
                            LogLookupDebug(this, lookup, LookupDebugType.Hit);
                            return SkinUtils.As<TValue>(new Bindable<IReadOnlyList<Color4>?>(Configuration.ComboColours));
                        }
                    }

                    break;

                case SkinComboColourLookup comboColour:
                    LogLookupDebug(this, lookup, LookupDebugType.Hit);
                    return SkinUtils.As<TValue>(new Bindable<Color4>(getComboColour(Configuration, comboColour.ColourIndex)));
            }

            LogLookupDebug(this, lookup, LookupDebugType.Miss);
            return null;
        }

        private static Color4 getComboColour(IHasComboColours source, int colourIndex)
            => source.ComboColours![colourIndex % source.ComboColours.Count];
    }
}
