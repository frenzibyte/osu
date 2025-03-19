// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
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

        private Container onlineRatingsWedge = null!;
        private Container failRetryWedge = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Width = 0.88f;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new ShearAlignedFlowContainer(shear)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 4f),
                Children = new Drawable[]
                {
                    new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
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
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 35, Vertical = 16 },
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
                    onlineRatingsWedge = new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
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
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
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
                    failRetryWedge = new Container
                    {
                        CornerRadius = 10,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = shear,
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
                                Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 40f, Vertical = 16 },
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
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            creator.Data = (metadata.Author.Username, new LinkDetails(LinkAction.OpenUserProfile, metadata.Author.Username));

            if (!string.IsNullOrEmpty(metadata.Source))
                source.Data = (metadata.Source, new LinkDetails(LinkAction.SearchBeatmapSet, metadata.Source));
            else
                source.Data = ("-", null);

            tag.Tags = metadata.Tags.Split(' ');
            submitted.Date = beatmapSetInfo.DateSubmitted ?? DateTimeOffset.Now;
            ranked.Date = beatmapSetInfo.DateRanked ?? DateTimeOffset.Now;

            if (currentOnlineBeatmapSet == null || currentOnlineBeatmapSet.OnlineID != beatmapSetInfo.OnlineID)
                refetchBeatmapSet();

            updateOnlineDisplay();
        }

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        private void refetchBeatmapSet()
        {
            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            currentRequest?.Cancel();
            currentRequest = null;
            currentOnlineBeatmapSet = null;

            if (beatmapSetInfo.OnlineID >= 1)
            {
                currentRequest = new GetBeatmapSetRequest(beatmapSetInfo.OnlineID);
                currentRequest.Success += s =>
                {
                    currentOnlineBeatmapSet = s;
                    updateOnlineDisplay();
                };

                api.Queue(currentRequest);
            }
        }

        private void updateOnlineDisplay()
        {
            if (currentRequest?.CompletionState == APIRequestCompletionState.Waiting)
            {
                genre.Data = null;
                language.Data = null;
            }
            else if (currentOnlineBeatmapSet == null)
            {
                genre.Data = ("-", null);
                language.Data = ("-", null);
                successRate.Data = (0, 0);
                userRating.Data = Array.Empty<int>();
                ratingSpread.Data = Array.Empty<int>();
                failRetryGraph.Data = new APIFailTimes();

                onlineRatingsWedge.FadeOut(300, Easing.OutQuint);
                onlineRatingsWedge.MoveToX(-50, 300, Easing.OutQuint);
                failRetryWedge.FadeOut(300, Easing.OutQuint);
                failRetryWedge.MoveToX(-50, 300, Easing.OutQuint);
            }
            else
            {
                var beatmapInfo = beatmap.Value.BeatmapInfo;

                var onlineBeatmapSet = currentOnlineBeatmapSet;
                var onlineBeatmap = onlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == beatmapInfo.OnlineID);

                genre.Data = (onlineBeatmapSet.Genre.Name, new LinkDetails(LinkAction.SearchBeatmapSet, onlineBeatmapSet.Genre.Name));
                language.Data = (onlineBeatmapSet.Language.Name, new LinkDetails(LinkAction.SearchBeatmapSet, onlineBeatmapSet.Language.Name));
                userRating.Data = onlineBeatmapSet.Ratings;
                ratingSpread.Data = onlineBeatmapSet.Ratings;

                if (onlineBeatmap != null)
                {
                    successRate.Data = (onlineBeatmap.PassCount, onlineBeatmap.PlayCount);
                    failRetryGraph.Data = onlineBeatmap.FailTimes ?? new APIFailTimes();
                }

                onlineRatingsWedge.FadeIn(300, Easing.OutQuint);
                onlineRatingsWedge.MoveToX(0, 300, Easing.OutQuint);
                failRetryWedge.FadeIn(300, Easing.OutQuint);
                failRetryWedge.MoveToX(0, 300, Easing.OutQuint);
            }
        }
    }
}
