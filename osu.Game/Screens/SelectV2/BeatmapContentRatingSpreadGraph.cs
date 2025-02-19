// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentRatingSpreadGraph : CompositeDrawable
    {
        private const float min_height = 4f;
        private const float max_height = 33f;

        private const int rating_range = 10;

        private readonly GraphBar[] graph;

        public int[] Ratings
        {
            set
            {
                if (!value.Any())
                {
                    foreach (var bar in graph)
                        bar.Height = min_height;
                }
                else
                {
                    var usableRange = value.Skip(1).Take(rating_range); // adjust for API returning weird empty data at 0.
                    int maxRating = usableRange.Max();

                    for (int i = 0; i < graph.Length; i++)
                        graph[i].Height = min_height + (max_height - min_height) * usableRange.ElementAt(i) / maxRating;
                }
            }
        }

        public BeatmapContentRatingSpreadGraph()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            graph = new[]
            {
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
                new GraphBar(),
            };

            graph = Enumerable.Range(0, rating_range).Select(_ => new GraphBar()).ToArray();

            InternalChildren = new[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 40f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 1f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = BeatmapsetsStrings.ShowStatsRatingSpread,
                            Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[] { new Dimension(GridSizeMode.Absolute, max_height) },
                            ColumnDimensions = graph.SkipLast(1).Select(_ => new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 2f),
                            }).SelectMany(d => d).Append(new Dimension()).ToArray(),
                            Content = new[]
                            {
                                graph.SkipLast(1).Select(g => new[]
                                {
                                    g,
                                    Empty()
                                }).SelectMany(g => g).Append(graph[^1]).ToArray()
                            },
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            for (int i = 0; i < 10; i++)
            {
                var left = Interpolation.ValueAt(i, colours.Blue4, colours.Blue0, 0, 10);
                var right = Interpolation.ValueAt(i + 1, colours.Blue4, colours.Blue0, 0, 10);
                graph[i].Colour = ColourInfo.GradientHorizontal(left, right);
            }
        }

        private partial class GraphBar : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;

                RelativeSizeAxes = Axes.X;
                CornerRadius = 2f;
                Masking = true;

                InternalChild = new Box { RelativeSizeAxes = Axes.Both };
            }
        }
    }
}
