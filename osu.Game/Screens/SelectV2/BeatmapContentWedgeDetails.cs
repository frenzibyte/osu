// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
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
                                Padding = new MarginPadding { Left = SongSelectV2.WEDGE_CONTENT_MARGIN + 14, Right = 35, Vertical = 16 },
                                Children = new[]
                                {
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 15),
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
                                                        creator = new BeatmapContentWedgeStatistic("Creator"),
                                                        source = new BeatmapContentWedgeStatistic("Source"),
                                                        genre = new BeatmapContentWedgeStatistic("Genre"),
                                                        Empty().With(e => e.Height = 15),
                                                        language = new BeatmapContentWedgeStatistic("Language"),
                                                        tag = new BeatmapContentWedgeStatistic("Tag", 40),
                                                        Empty().With(e => e.Height = 15),
                                                        submitted = new BeatmapContentWedgeStatistic("Submitted"),
                                                        ranked = new BeatmapContentWedgeStatistic("Ranked"),
                                                    },
                                                },
                                                Empty(),
                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Shear = shear,
                                                    Direction = FillDirection.Vertical,
                                                    Spacing = new Vector2(0f, 15f),
                                                    Children = new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Padding = new MarginPadding { Left = -22 },
                                                            Shear = -shear,
                                                            Child = successRate = new BeatmapContentSuccessRateBar(),
                                                        },
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Padding = new MarginPadding { Left = -11 },
                                                            Shear = -shear,
                                                            Child = userRating = new BeatmapContentUserRatingBar(),
                                                        },
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Shear = -shear,
                                                            Child = ratingSpread = new BeatmapContentRatingSpreadGraph(),
                                                        },
                                                    },
                                                },
                                            },
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
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -shear,
                                Padding = new MarginPadding { Left = SongSelectV2.WEDGE_CONTENT_MARGIN + 53, Right = 40f, Vertical = 16 },
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

            creator.Value = ("Sotarks", new LinkDetails(LinkAction.OpenUserProfile, "Sotarks"));
            source.Value = ("REFLEC BEAT limelight", new LinkDetails(LinkAction.SearchBeatmapSet, "REFLEC BEAT limelight"));
            genre.Value = ("Video Game (Instrumental)", new LinkDetails(LinkAction.SearchBeatmapSet, "Video Game (Instrumental)"));
            language.Value = ("Instrumental", new LinkDetails(LinkAction.SearchBeatmapSet, "Instrumental"));
            tag.Tags = new[]
            {
                "risk", "junk", "beatmania", "iidx", "19", "lincle", "jubeat", "saucer", "jukebeat", "konami", "music", "pack", "test", "something", "like", "your", "yeah", "andddd", "then", "it",
                "goes"
                // "risk junk beatmania iidx 19 lincle jubeat saucer jukebeat konami music pack test something like your yeah and then it goes"
            };
            submitted.Date = new DateTime(2018, 11, 4);
            ranked.Date = new DateTime(2019, 1, 6);

            successRate.Value = 0.9453f;
            userRating.Ratings = new[] { 1, 2, 50, 340, 29, 3, 9, 200, 503, 932, 32 };
            ratingSpread.Ratings = new[] { 1, 2, 50, 340, 29, 3, 9, 200, 503, 932, 32 };

            int[] retries = Enumerable.Range(0, 150).Select(_ => RNG.Next(50, 100)).ToArray();
            int[] fails = Enumerable.Range(0, 150).Select(_ => RNG.Next(50, 100)).ToArray();
            failRetryGraph.Data = (retries, fails);
        }
    }
}
