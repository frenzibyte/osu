// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedgeDetails : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapContentWedgeStatistic creator = null!;
        private BeatmapContentWedgeStatistic source = null!;
        private BeatmapContentWedgeStatistic genre = null!;
        private BeatmapContentWedgeStatistic language = null!;
        private BeatmapContentWedgeStatistic tag = null!;
        private BeatmapContentWedgeStatistic submitted = null!;
        private BeatmapContentWedgeStatistic ranked = null!;

        private BeatmapContentSuccessRateBar successRate = null!;
        private BeatmapContentUserRatingBar userRating = null!;
        private BeatmapContentRatingSpreadGraph ratingSpread = null!;

        private BeatmapContentFailRetryGraph failRetryGraph = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 4f),
                Children = new[]
                {
                    new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN + 14, Right = 35, Vertical = 16 },
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0f, 10f),
                                        Children = new Drawable[]
                                        {
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(),
                                                    new Dimension(),
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
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                creator = new BeatmapContentWedgeStatistic("Creator"),
                                                                genre = new BeatmapContentWedgeStatistic("Genre"),
                                                            },
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                source = new BeatmapContentWedgeStatistic("Source"),
                                                                language = new BeatmapContentWedgeStatistic("Language"),
                                                            },
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Vertical,
                                                            Spacing = new Vector2(0f, 10f),
                                                            Children = new[]
                                                            {
                                                                submitted = new BeatmapContentWedgeStatistic("Submitted"),
                                                                ranked = new BeatmapContentWedgeStatistic("Ranked"),
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                            tag = new BeatmapContentWedgeStatistic("Tags"),
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5,
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
                                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 10),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 10),
                                    new Dimension(),
                                },
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN + 40, Right = 40f, Vertical = 16 },
                                Content = new[]
                                {
                                    new[]
                                    {
                                        successRate = new BeatmapContentSuccessRateBar(),
                                        Empty(),
                                        userRating = new BeatmapContentUserRatingBar(),
                                        Empty(),
                                        ratingSpread = new BeatmapContentRatingSpreadGraph(),
                                    },
                                },
                            },
                        }
                    },
                    new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN + 60, Right = 40f, Vertical = 16 },
                                Child = failRetryGraph = new BeatmapContentFailRetryGraph(),
                            },
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.BindValueChanged(_ => updateDisplay(), true);
        }

        [Resolved]
        private BeatmapLookupCache beatmapCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private void updateDisplay()
        {
            var metadata = beatmap.Value.Metadata;
            var beatmapInfo = beatmap.Value.BeatmapInfo;
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            creator.Value = (metadata.Author.Username, new LinkDetails(LinkAction.OpenUserProfile, metadata.Author.Username));

            if (!string.IsNullOrEmpty(metadata.Source))
                source.Value = (metadata.Source, new LinkDetails(LinkAction.SearchBeatmapSet, metadata.Source));
            else
                source.Value = ("-", null);

            tag.Tags = metadata.Tags.Split(' ');
            submitted.Date = beatmapSetInfo.DateSubmitted ?? DateTimeOffset.Now;
            ranked.Date = beatmapSetInfo.DateRanked ?? DateTimeOffset.Now;

            updateOnlineDisplay();

            if (beatmapInfo.OnlineID >= 1)
            {
            }
            else
            {
                genre.Value = ("-", null);
                language.Value = ("-", null);
            }
        }

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        private void updateOnlineDisplay()
        {
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            currentRequest?.Cancel();
            currentRequest = null;

            if (beatmapSetInfo.OnlineID < 1)
            {
                genre.Value = ("-", null);
                language.Value = ("-", null);
                userRating.Ratings = Array.Empty<int>();
                ratingSpread.Ratings = Array.Empty<int>();
                successRate.Value = 0;
                failRetryGraph.Data = (Array.Empty<int>(), Array.Empty<int>());
            }
            else if (currentOnlineBeatmapSet == null || currentOnlineBeatmapSet.OnlineID != beatmapSetInfo.OnlineID)
            {
                genre.Value = null;
                language.Value = null;
                userRating.Ratings = Array.Empty<int>();
                ratingSpread.Ratings = Array.Empty<int>();
                successRate.Value = 0;
                failRetryGraph.Data = (Array.Empty<int>(), Array.Empty<int>());

                currentRequest = new GetBeatmapSetRequest(beatmapSetInfo.OnlineID);
                currentRequest.Success += s =>
                {
                    currentOnlineBeatmapSet = s;

                    if (!string.IsNullOrEmpty(s.Genre.Name))
                        genre.Value = (s.Genre.Name, new LinkDetails(LinkAction.SearchBeatmapSet, s.Genre.Name));
                    else
                        genre.Value = ("-", null);

                    if (!string.IsNullOrEmpty(s.Language.Name))
                        language.Value = (s.Language.Name, new LinkDetails(LinkAction.SearchBeatmapSet, s.Language.Name));
                    else
                        language.Value = ("-", null);

                    userRating.Ratings = s.Ratings;
                    ratingSpread.Ratings = s.Ratings;

                    updateOnlineBeatmap();
                };

                api.Queue(currentRequest);
            }
            else
                updateOnlineBeatmap();
        }

        private void updateOnlineBeatmap()
        {
            var beatmapInfo = beatmap.Value.BeatmapInfo;

            Debug.Assert(currentOnlineBeatmapSet != null);
            var onlineBeatmap = currentOnlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

            if (onlineBeatmap != null)
            {
                successRate.Value = (float)onlineBeatmap.PassCount / onlineBeatmap.PlayCount;

                failRetryGraph.Data = (
                    onlineBeatmap.FailTimes?.Retries ?? Array.Empty<int>(),
                    onlineBeatmap.FailTimes?.Fails ?? Array.Empty<int>());
            }
        }
    }
}
