// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDifficultyWedge : CompositeDrawable
    {
        private const float border_weight = 2;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Box difficultyBorder = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText mappedByText = null!;
        private OsuSpriteText mapperText = null!;

        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> beatmapStatisticsFlow = null!;
        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> difficultyStatisticsFlow = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public BeatmapDifficultyWedge()
        {
            Width = 650;
            Height = 80;
            X = -18;
            Y = 232;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10;
            Shear = shear;

            beatmap.Value.Beatmap.GetStatistics();

            InternalChildren = new Drawable[]
            {
                difficultyBorder = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    Height = 28f,
                    Direction = FillDirection.Horizontal,
                    Shear = -shear,
                    Margin = new MarginPadding { Left = SongSelectV2.WEDGE_CONTENT_MARGIN },
                    Spacing = new Vector2(8f, 0f),
                    Children = new Drawable[]
                    {
                        starRatingDisplay = new StarRatingDisplay(default, animated: false, darkBackground: true)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5f, 0f),
                            Margin = new MarginPadding { Bottom = 2f },
                            Children = new[]
                            {
                                difficultyText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = "Nasty Normal",
                                    Font = OsuFont.Torus.With(size: 19.2f, weight: FontWeight.SemiBold),
                                    Colour = Color4.Black.Opacity(0.75f),
                                },
                                mappedByText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = "mapped by",
                                    Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.Regular),
                                    Colour = Color4.Black.Opacity(0.75f),
                                },
                                mapperText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = "mapper name",
                                    Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.SemiBold),
                                    Colour = Color4.Black.Opacity(0.75f),
                                },
                            },
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 28f, Bottom = border_weight, Right = border_weight },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10 - border_weight,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background5,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.2f,
                                Colour = ColourInfo.GradientHorizontal(Color4.Transparent, colours.Orange1),
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(16f, 0f),
                                Margin = new MarginPadding { Left = SongSelectV2.WEDGE_CONTENT_MARGIN + 6, Top = 7.5f },
                                Children = new Drawable[]
                                {
                                    beatmapStatisticsFlow = new FillFlowContainer<BeatmapDifficultyWedgeStatistic>
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Shear = -shear,
                                        Spacing = new Vector2(8f, 0f),
                                    },
                                    difficultyStatisticsFlow = new FillFlowContainer<BeatmapDifficultyWedgeStatistic>
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Shear = -shear,
                                        Spacing = new Vector2(8f, 0f),
                                        Children = new[]
                                        {
                                            new BeatmapDifficultyWedgeStatistic("Circle Size") { Value = ("2.7", 2.7f, 10f) },
                                            new BeatmapDifficultyWedgeStatistic("Accuracy") { Value = ("3", 3f, 10f) },
                                            new BeatmapDifficultyWedgeStatistic("HP Drain") { Value = ("2", 2f, 10f) },
                                            new BeatmapDifficultyWedgeStatistic("Approach Rate") { Value = ("4", 4f, 10f) },
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            mods.BindValueChanged(_ => updateDisplay());
            updateDisplay();

            displayedStars.BindValueChanged(_ => updateStars(), true);
        }

        private void updateDisplay()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            computeStarDifficulty(cancellationSource.Token);

            // beatmapStatisticsFlow.Children = beatmap.Value.Beatmap.GetStatistics().Select(s => new BeatmapDifficultyWedgeStatistic(s.Name)
            // {
            //     Value = (s.Content, 1f, 1f),
            // }).ToArray();
            beatmapStatisticsFlow.Children = new[]
            {
                new BeatmapStatistic
                {
                    Name = BeatmapsetsStrings.ShowStatsCountCircles,
                    Content = "320",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                },
                new BeatmapStatistic
                {
                    Name = BeatmapsetsStrings.ShowStatsCountSliders,
                    Content = "120",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Sliders),
                },
                new BeatmapStatistic
                {
                    Name = @"Spinner Count",
                    Content = "4",
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                }
            }.Select(s => new BeatmapDifficultyWedgeStatistic(s.Name)
            {
                Value = (s.Content, 1f, 1f),
            }).ToArray();
        }

        private void updateStars()
        {
            difficultyBorder.Colour = colours.ForStarDifficulty(displayedStars.Value);
            starRatingDisplay.Current.Value = new StarDifficulty(displayedStars.Value, 0);

            foreach (var statistic in beatmapStatisticsFlow.Concat(difficultyStatisticsFlow))
                statistic.AccentColour = colours.ForStarDifficulty(displayedStars.Value);
        }

        private void computeStarDifficulty(CancellationToken cancellationToken)
        {
            difficultyCache.GetDifficultyAsync(beatmap.Value.BeatmapInfo, ruleset.Value, mods.Value, cancellationToken)
                           .ContinueWith(task =>
                           {
                               Schedule(() =>
                               {
                                   if (cancellationToken.IsCancellationRequested)
                                       return;

                                   var result = task.GetResultSafely() ?? default;
                                   this.TransformBindableTo(displayedStars, result.Stars, StarRatingDisplay.TRANSITION_DURATION, StarRatingDisplay.TRANSITION_EASING);
                               });
                           }, cancellationToken);
        }
    }
}
