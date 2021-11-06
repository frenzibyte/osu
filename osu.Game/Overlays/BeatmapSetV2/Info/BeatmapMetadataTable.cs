// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.BeatmapSetV2.Info.Metadata;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    /// <summary>
    /// A composite displaying all of the beatmap's metadata and statistics in a 3-columns grid container.
    /// </summary>
    public class BeatmapMetadataTable : CompositeDrawable
    {
        [Resolved]
        private IBindable<APIBeatmap> beatmap { get; set; }

        [Resolved(canBeNull: true)]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [Resolved(canBeNull: true)]
        private IBindable<IReadOnlyList<APIBeatmap>> availableBeatmaps { get; set; }

        private BeatmapLinkedMetadataItem creator;
        private BeatmapLinkedMetadataItem source;
        private BeatmapLinkedMetadataItem genre;
        private BeatmapLinkedMetadataItem language;
        private BeatmapLinkedMetadataItem tags;
        private BeatmapLinkedMetadataItem nominators;
        private BeatmapDateTimeMetadataItem submitted;
        private BeatmapDateTimeMetadataItem ranked;

        private BeatmapMetadataPill circleSliderCountPill;

        private BeatmapNumberMetadataItem<int> circleCount;
        private BeatmapNumberMetadataItem<int> sliderCount;

        private BeatmapNumberMetadataItem<float> circleSize;
        private BeatmapNumberMetadataItem<float> drainRate;
        private BeatmapNumberMetadataItem<float> overallDifficulty;
        private BeatmapNumberMetadataItem<float> approachRate;

        private BeatmapNumberMetadataItem<double> starDifficulty;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 50f),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 30f),
                    new Dimension(),
                },
                Content = new[]
                {
                    new[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Children = new[]
                            {
                                creator = new BeatmapLinkedMetadataItem("Creator", LinkAction.OpenUserProfile, ", "),
                                source = new BeatmapLinkedMetadataItem("Source", LinkAction.SearchBeatmapSet),
                                genre = new BeatmapLinkedMetadataItem("Genre", LinkAction.SearchBeatmapSet),
                                Empty().With(d => d.Height = 10f),
                                language = new BeatmapLinkedMetadataItem("Language", LinkAction.SearchBeatmapSet),
                                tags = new BeatmapLinkedMetadataItem("Tags", LinkAction.SearchBeatmapSet, height: 25),
                                Empty().With(d => d.Height = 10f),
                                nominators = new BeatmapLinkedMetadataItem("Nominators", LinkAction.OpenUserProfile, ", "),
                                submitted = new BeatmapDateTimeMetadataItem("Submitted"),
                                ranked = new BeatmapDateTimeMetadataItem("Ranked"),
                            }
                        },
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 2f,
                            RelativeSizeAxes = Axes.Y,
                            Colour = colourProvider.Background4,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0f, 12.5f),
                                    Children = new Drawable[]
                                    {
                                        circleSliderCountPill = new BeatmapMetadataPill
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 20f,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Children = new[]
                                            {
                                                circleCount = new BeatmapNumberMetadataItem<int>("Circle Count"),
                                                sliderCount = new BeatmapNumberMetadataItem<int>("Slider Count"),
                                                circleSize = new BeatmapNumberMetadataItem<float>("Circle Size", "0.##") { MaxValue = 10.0f },
                                                drainRate = new BeatmapNumberMetadataItem<float>("HP Drain", "0.##") { MaxValue = 10.0f },
                                                Empty().With(d => d.Height = 10f),
                                                overallDifficulty = new BeatmapNumberMetadataItem<float>("Accuracy", "0.##") { MaxValue = 10.0f },
                                                approachRate = new BeatmapNumberMetadataItem<float>("Approach Rate", "0.##") { MaxValue = 10.0f },
                                                starDifficulty = new BeatmapNumberMetadataItem<double>("Star Difficulty", "0.00") { MaxValue = 10.0f },
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        Empty(),
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 16f),
                            Children = new Drawable[]
                            {
                                new BeatmapSuccessRateStatisticItem { Current = { BindTarget = beatmap } },
                                new BeatmapUserRatingStatisticItem { Current = { BindTarget = beatmapSet } },
                                new BeatmapRatingSpreadStatisticItem { Current = { BindTarget = beatmapSet } },
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                // todo: pending web-side support
                nominators.Value = string.Empty;

                circleSliderCountPill.Values = new[]
                {
                    ("Circle Count", b.NewValue?.CircleCount.ToString()),
                    ("Slider Count", b.NewValue?.SliderCount.ToString()),
                };

                circleCount.Value = b.NewValue?.CircleCount;
                sliderCount.Value = b.NewValue?.SliderCount;

                circleSize.Value = b.NewValue?.CircleSize;
                drainRate.Value = b.NewValue?.DrainRate;
                overallDifficulty.Value = b.NewValue?.OverallDifficulty;
                approachRate.Value = b.NewValue?.ApproachRate;

                starDifficulty.Value = b.NewValue?.StarRating;
            }, true);

            beatmapSet.BindValueChanged(set =>
            {
                creator.Value = set.NewValue?.AuthorString ?? "-";
                source.Value = set.NewValue?.Source ?? "-";
                genre.Value = set.NewValue?.Genre.Name ?? "-";

                language.Value = set.NewValue?.Language.Name ?? "-";
                tags.Values = set.NewValue?.Tags.Split(' ') ?? new[] { "-" };

                submitted.Value = set.NewValue?.Submitted;
                ranked.Value = set.NewValue?.Ranked;
            }, true);

            availableBeatmaps.BindValueChanged(available =>
            {
                circleCount.MinValue = available.NewValue.Select(bi => bi.CircleCount).DefaultIfEmpty(0).Min();
                sliderCount.MinValue = available.NewValue.Select(bi => bi.SliderCount).DefaultIfEmpty(0).Min();
                circleCount.MaxValue = available.NewValue.Select(bi => bi.CircleCount).DefaultIfEmpty(1).Max();
                sliderCount.MaxValue = available.NewValue.Select(bi => bi.SliderCount).DefaultIfEmpty(1).Max();
            }, true);
        }
    }
}
