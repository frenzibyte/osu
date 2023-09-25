// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonCurvedHealthDisplay : HealthDisplay
    {
        private const float bar_length = 300;
        private const float bar_height = 35;
        private const float curve_start = 220;
        private const float curve_end = 250;

        private SmoothPath hurtBar = null!;
        private SmoothPath healthBar = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4f, 0f),
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Margin = new MarginPadding { Top = 10f - 1.5f },
                        Size = new Vector2(48f, 3f),
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new BackgroundPath
                            {
                                PathRadius = 10f,
                                BorderColour = Color4.White.Opacity(0.3f),
                                Vertices = PathApproximator.ApproximateBezier(new[]
                                {
                                    // todo: this is also silly
                                    new Vector2(0, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_start, 0),
                                    new Vector2(curve_end, bar_height),
                                    new Vector2(curve_end, bar_height),
                                    new Vector2(curve_end, bar_height),
                                    new Vector2(curve_end, bar_height),
                                    new Vector2(bar_length, bar_height),
                                })
                            },
                            // todo: implement glow
                            // new SmoothPath
                            // {
                            //     Name = "\"Hurt\" Bar Glow",
                            //     Anchor = Anchor.TopLeft,
                            //     Origin = Anchor.TopLeft,
                            //     PathRadius = 7.5f,
                            //     Margin = new MarginPadding { Left = 2.5f, Top = 2.5f },
                            //     Vertices = PathApproximator.ApproximateBezier(new[]
                            //     {
                            //         // todo: this is also silly
                            //         new Vector2(0, 0),
                            //         new Vector2(200f, 0),
                            //     })
                            // },
                            hurtBar = new SmoothPath
                            {
                                Alpha = 0f,
                                Name = "\"Hurt\" Bar",
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Colour = Color4Extensions.FromHex("#FF9393"),
                                PathRadius = 5f,
                                Margin = new MarginPadding { Left = 5f, Top = 5f },
                                Vertices = PathApproximator.ApproximateBezier(new[]
                                {
                                    // todo: this is also silly
                                    new Vector2(0, 0),
                                    new Vector2(200f, 0),
                                })
                            },
                            // todo: implement glow
                            // new SmoothPath
                            // {
                            //     Name = "Health Bar Glow",
                            //     Anchor = Anchor.TopLeft,
                            //     Origin = Anchor.TopLeft,
                            //     Colour = Color4Extensions.FromHex("#7ED7FD"),
                            //     Alpha = 0f,
                            //     PathRadius = 7.5f,
                            //     Margin = new MarginPadding { Left = 2.5f, Top = 2.5f },
                            //     Vertices = PathApproximator.ApproximateBezier(new[]
                            //     {
                            //         // todo: this is also silly
                            //         new Vector2(0, 0),
                            //         new Vector2(150f, 0),
                            //     })
                            // },
                            healthBar = new SmoothPath
                            {
                                Name = "Health Bar",
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                PathRadius = 5f,
                                Margin = new MarginPadding { Left = 5f, Top = 5f },
                                Vertices = PathApproximator.ApproximateBezier(new[]
                                {
                                    // todo: this is also silly
                                    new Vector2(0, 0),
                                    new Vector2(150f, 0),
                                })
                            },
                        }
                    }
                },
            };
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
            if (hurtBar.Alpha == 1f)
                return;

            float length = (float)oldHealth * bar_length;

            // todo: this animation is jank lol
            hurtBar.Vertices = PathApproximator.ApproximateBezier(generateCurve(length).ToArray());
            hurtBar.FadeIn().Delay(500).FadeOut(300, Easing.OutQuint);
        }

        private void updateHealthBar()
        {
            double health = Current.Value;
            float length = (float)health * bar_length;

            healthBar.Alpha = length > 0f ? 1f : 0f;
            healthBar.Vertices = PathApproximator.ApproximateBezier(generateCurve(length).ToArray());
        }

        private static List<Vector2> generateCurve(float length)
        {
            var vertices = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(MathF.Min(length, curve_start), 0)
            };

            if (length > curve_start)
            {
                vertices.Add(new Vector2(curve_start, 0));
                vertices.Add(new Vector2(curve_start, 0));
                vertices.Add(new Vector2(curve_start, 0));
                vertices.Add(new Vector2(curve_start, 0));
                vertices.Add(new Vector2(curve_start, 0));
                vertices.Add(new Vector2(MathF.Min(length, curve_end), bar_height * ((MathF.Min(length, curve_end) - curve_start) / (curve_end - curve_start))));
            }

            if (length > curve_end)
            {
                vertices.Add(new Vector2(curve_end, bar_height));
                vertices.Add(new Vector2(curve_end, bar_height));
                vertices.Add(new Vector2(curve_end, bar_height));

                if (length > curve_end + 25)
                {
                    vertices.Add(new Vector2(length, bar_height));
                }
                else
                    vertices.Add(new Vector2(length, bar_height));
            }

            return vertices;
        }

        // todo: this should be shared with DrawableSliderPath, as a BorderedPath class kinda thing.
        private partial class BackgroundPath : SmoothPath
        {
            public const float BORDER_PORTION = 0.128f;
            public const float GRADIENT_PORTION = 1 - BORDER_PORTION;

            private const float border_max_size = 8f;
            private const float border_min_size = 0f;

            private Color4 borderColour = Color4.White;

            public Color4 BorderColour
            {
                get => borderColour;
                set
                {
                    if (borderColour == value)
                        return;

                    borderColour = value;

                    InvalidateTexture();
                }
            }

            private Color4 accentColour = Color4.White;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    if (accentColour == value)
                        return;

                    accentColour = value;

                    InvalidateTexture();
                }
            }

            private float borderSize = 1;

            public float BorderSize
            {
                get => borderSize;
                set
                {
                    if (borderSize == value)
                        return;

                    if (value < border_min_size || value > border_max_size)
                        return;

                    borderSize = value;

                    InvalidateTexture();
                }
            }

            protected float CalculatedBorderPortion => BorderSize * BORDER_PORTION;

            private const float opacity_at_centre = 0.3f;
            private const float opacity_at_edge = 0.8f;

            protected override Color4 ColourAt(float position)
            {
                if (CalculatedBorderPortion != 0f && position <= CalculatedBorderPortion)
                    return BorderColour;

                position -= CalculatedBorderPortion;
                return Interpolation.ValueAt(Math.Clamp(position, 0f, 1f), Color4.White.Opacity(0.5f), Color4.Black.Opacity(0.5f), -0.75f, 1f, Easing.OutQuart);
            }
        }
    }
}