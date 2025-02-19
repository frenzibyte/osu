// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentFailRetryGraph : CompositeDrawable
    {
        private readonly GraphDrawable retriesGraph;
        private readonly GraphDrawable failsGraph;
        private readonly Circle bottomBar;

        public (int[] retries, int[] fails) Data
        {
            set
            {
                int[] total = value.retries.Zip(value.fails).Select(p => p.First + p.Second).ToArray();
                int maximum = total.Max();

                retriesGraph.Data = total.Select(r => (float)r / maximum).ToArray();
                failsGraph.Data = value.fails.Select(r => (float)r / maximum).ToArray();
            }
        }

        public BeatmapContentFailRetryGraph()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 4f),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = BeatmapsetsStrings.ShowInfoPointsOfFailure,
                        Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                        Margin = new MarginPadding { Bottom = 4f },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 65f,
                        Children = new[]
                        {
                            retriesGraph = new GraphDrawable { RelativeSizeAxes = Axes.Both, Y = -1f },
                            failsGraph = new GraphDrawable { RelativeSizeAxes = Axes.Both },
                        },
                    },
                    bottomBar = new Circle
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 3f,
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            retriesGraph.Colour = colours.Orange1;
            failsGraph.Colour = colours.DarkOrange2;
            bottomBar.Colour = colourProvider.Background6;
        }

        private partial class GraphDrawable : Drawable
        {
            public float[] Data = new float[10];

            protected override DrawNode CreateDrawNode() => new GraphDrawNode(this);

            private class GraphDrawNode : DrawNode
            {
                private readonly GraphDrawable source;

                private Vector2 drawSize;
                private float[] data = null!;

                public GraphDrawNode(GraphDrawable source)
                    : base(source)
                {
                    this.source = source;
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    drawSize = source.DrawSize;
                    data = source.Data;
                }

                protected override void Draw(IRenderer renderer)
                {
                    base.Draw(renderer);

                    // todo: try moving this into BarGraph
                    const float spacing_constant = 1.5f;

                    float position = 0;
                    float barWidth = drawSize.X / data.Length / spacing_constant;

                    Debug.Assert(data.Length > 1);
                    float totalSpacing = drawSize.X - barWidth * data.Length;
                    float spacing = totalSpacing / (data.Length - 1);

                    for (int i = 0; i < data.Length; i++)
                    {
                        float barHeight = MathF.Max(drawSize.Y * data[i], barWidth);

                        drawBar(renderer, position, barWidth, barHeight);

                        position += barWidth + spacing;
                    }
                }

                private void drawBar(IRenderer renderer, float position, float width, float height)
                {
                    float cornerRadius = width / 2f;

                    Vector3 scale = DrawInfo.MatrixInverse.ExtractScale();
                    float blendRange = (scale.X + scale.Y) / 2;

                    RectangleF drawRectangle = new RectangleF(new Vector2(position, drawSize.Y - height), new Vector2(width, height));
                    Quad screenSpaceDrawQuad = Quad.FromRectangle(drawRectangle) * DrawInfo.Matrix;

                    renderer.PushMaskingInfo(new MaskingInfo
                    {
                        ScreenSpaceAABB = screenSpaceDrawQuad.AABB,
                        MaskingRect = drawRectangle.Normalize(),
                        ConservativeScreenSpaceQuad = screenSpaceDrawQuad,
                        ToMaskingSpace = DrawInfo.MatrixInverse,
                        CornerRadius = cornerRadius,
                        CornerExponent = 2f,
                        // We are setting the linear blend range to the approximate size of a _pixel_ here.
                        // This results in the optimal trade-off between crispness and smoothness of the
                        // edges of the masked region according to sampling theory.
                        BlendRange = blendRange,
                        AlphaExponent = 1,
                    });

                    renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour);
                    renderer.PopMaskingInfo();
                }
            }
        }
    }
}
