// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthBar : SmoothPath
    {
        public const float PATH_RADIUS = 10f;

        private const float curve_start_offset = 70;
        private const float curve_end_offset = 40;
        private const float padding = PATH_RADIUS * 2;
        private const float curve_smoothness = 10;

        private Colour4 barColour;

        public Colour4 BarColour
        {
            get => barColour;
            set
            {
                if (barColour == value)
                    return;

                barColour = value;
                InvalidateTexture();
            }
        }

        private Colour4 glowColour;

        public Colour4 GlowColour
        {
            get => glowColour;
            set
            {
                if (glowColour == value)
                    return;

                glowColour = value;
                InvalidateTexture();
            }
        }

        public float GlowPortion { get; init; }

        private double startValue;

        public double StartValue
        {
            get => startValue;
            set
            {
                startValue = value;
                updateVertices();
            }
        }

        private double endValue = 1.0;

        public double EndValue
        {
            get => endValue;
            set
            {
                endValue = value;
                updateVertices();
            }
        }

        public IBindable<float> BarLength { get; } = new BindableFloat();
        public IBindable<float> BarHeight { get; } = new BindableFloat();

        private SliderPath? barPath;

        public ArgonHealthBar()
        {
            PathRadius = PATH_RADIUS;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BarLength.BindValueChanged(_ => recreatePath());
            BarHeight.BindValueChanged(_ => recreatePath(), true);
        }

        protected override Color4 ColourAt(float position)
        {
            if (position >= GlowPortion)
                return BarColour;

            return Interpolation.ValueAt(position, Colour4.Black.Opacity(0.0f), GlowColour, 0.0, GlowPortion, Easing.InQuint);
        }

        private void recreatePath()
        {
            // the display starts curving at `curve_start_offset` units from the right and ends curving at `curve_end_offset`.
            // to ensure that the curve is symmetric when it starts being narrow enough, add a `curve_end_offset` to the left side too.
            const float rescale_cutoff = curve_start_offset + curve_end_offset;

            float targetLength = Math.Max(BarLength.Value - padding, rescale_cutoff);
            float curveStart = targetLength - curve_start_offset;
            float curveEnd = targetLength - curve_end_offset;

            Vector2 diagonalDir = (new Vector2(curveEnd, BarHeight.Value) - new Vector2(curveStart, 0)).Normalized();

            barPath = new SliderPath(new[]
            {
                new PathControlPoint(new Vector2(0, 0), PathType.LINEAR),
                new PathControlPoint(new Vector2(curveStart - curve_smoothness, 0), PathType.BEZIER),
                new PathControlPoint(new Vector2(curveStart, 0)),
                new PathControlPoint(new Vector2(curveStart, 0) + diagonalDir * curve_smoothness, PathType.LINEAR),
                new PathControlPoint(new Vector2(curveEnd, BarHeight.Value) - diagonalDir * curve_smoothness, PathType.BEZIER),
                new PathControlPoint(new Vector2(curveEnd, BarHeight.Value)),
                new PathControlPoint(new Vector2(curveEnd + curve_smoothness, BarHeight.Value), PathType.LINEAR),
                new PathControlPoint(new Vector2(targetLength, BarHeight.Value)),
            });

            if (BarLength.Value - padding < rescale_cutoff)
                rescalePathProportionally(barPath, BarLength.Value, targetLength);

            updateVertices();
        }

        private static void rescalePathProportionally(SliderPath path, float currentLength, float targetLength)
        {
            foreach (var point in path.ControlPoints)
                point.Position = new Vector2(point.Position.X / targetLength * (currentLength - padding), point.Position.Y);
        }

        private readonly List<Vector2> vertices = new List<Vector2>();

        private void updateVertices()
        {
            vertices.Clear();
            barPath?.GetPathToProgress(vertices, StartValue, EndValue);

            if (vertices.Count > 0)
            {
                Vector2 initialVertex = vertices[0];
                for (int i = 0; i < vertices.Count; i++)
                    vertices[i] -= initialVertex;
            }
            else
                vertices.Add(Vector2.Zero);

            Vertices = vertices;
        }
    }
}
