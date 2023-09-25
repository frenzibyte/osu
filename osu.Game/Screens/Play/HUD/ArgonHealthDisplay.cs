// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthDisplay : HealthDisplay
    {
        private const float bar_length = 300;
        private const float bar_height = 35;
        private const float curve_start = 220;
        private const float curve_end = 250;

        // private SmoothPath hurtBar = null!;
        // private SmoothPath healthBar = null!;

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer)
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                new Circle
                {
                    Margin = new MarginPadding { Top = 10f - 1.5f },
                    Size = new Vector2(46f, 3f),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding { Horizontal = 50f },
                    Height = 20f,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Colour = Color4.White.Multiply(0.5f),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Scale = new Vector2(0.996f, 0.85f),
                            Children = new[]
                            {
                                new Sprite
                                {
                                    Texture = generateTexture(renderer),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(1f),
                                },
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Horizontal = 6.25f },
                            Children = new[]
                            {
                                new Circle
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativePositionAxes = Axes.X,
                                    RelativeSizeAxes = Axes.X,
                                    Scale = new Vector2(0.9875f, 1f),
                                    Colour = Color4Extensions.FromHex("#FF9393"),
                                    X = 0.49f,
                                    Width = 0.21f,
                                    Height = 10f,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Radius = 20f,
                                        Colour = Color4Extensions.FromHex("#FD0000"),
                                    },
                                },
                                new CircularContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new Circle
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0.5f,
                                        Height = 10f,
                                        EdgeEffect = new EdgeEffectParameters
                                        {
                                            Type = EdgeEffectType.Shadow,
                                            Radius = 10f,
                                            Colour = Color4Extensions.FromHex("#7ED7FD").Multiply(0.5f),
                                        },
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }

        private Texture generateTexture(IRenderer renderer)
        {
            var texture = renderer.CreateTexture(1, 100);
            var image = new Image<Rgba32>(1, 100);

            for (int i = 0; i < 50; i++)
            {
                Color4 colour = Interpolation.ValueAt(i, Color4.White.Multiply(0.5f), Color4.Black, -75, 49, Easing.OutQuad);

                image[0, i] = new Rgba32(colour.R, colour.G, colour.B, colour.A);
                image[0, 99 - i] = new Rgba32(colour.R, colour.G, colour.B, colour.A);
            }

            texture.SetData(new TextureUpload(image));
            return texture;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(v =>
            {
                if (v.NewValue < v.OldValue)
                    displayDecrease(v.OldValue);

                updateHealthBar();
            }, true);
        }

        private void displayDecrease(double oldHealth)
        {
            // if (hurtBar.Alpha == 1f)
            //     return;

            float length = (float)oldHealth * bar_length;

            // todo: this animation is jank lol
            // hurtBar.Vertices = PathApproximator.ApproximateBezier(generateCurve(length).ToArray());
            // hurtBar.FadeIn().Delay(500).FadeOut(300, Easing.OutQuint);
        }

        private void updateHealthBar()
        {
            double health = Current.Value;
            float length = (float)health * bar_length;

            // healthBar.Alpha = length > 0f ? 1f : 0f;
            // healthBar.Vertices = PathApproximator.ApproximateBezier(generateCurve(length).ToArray());
        }
    }
}