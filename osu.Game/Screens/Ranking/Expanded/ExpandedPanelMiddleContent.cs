// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Screens.Ranking.Expanded.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// The content that appears in the middle section of the <see cref="ScorePanel"/>.
    /// </summary>
    public class ExpandedPanelMiddleContent : CompositeDrawable
    {
        private const float padding = 10;

        private readonly IScoreInfo score;
        private readonly bool withFlair;

        private readonly List<StatisticDisplay> statisticDisplays = new List<StatisticDisplay>();

        private FillFlowContainer starAndModDisplay;
        private RollingCounter<long> scoreCounter;

        [Resolved]
        private ScoreManager scoreManager { get; set; }

        /// <summary>
        /// Creates a new <see cref="ExpandedPanelMiddleContent"/>.
        /// </summary>
        /// <param name="score">The score to display.</param>
        /// <param name="withFlair">Whether to add flair for a new score being set.</param>
        public ExpandedPanelMiddleContent(IScoreInfo score, bool withFlair = false)
        {
            this.score = score;
            this.withFlair = withFlair;

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Padding = new MarginPadding(padding);
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache beatmapDifficultyCache, RulesetStore rulesets)
        {
            var beatmap = score.Beatmap;
            var metadata = beatmap.BeatmapSet?.Metadata ?? beatmap.Metadata;
            string creator = metadata.Author.Username;

            int? beatmapMaxCombo = scoreManager.GetMaximumAchievableComboAsync(score).GetResultSafely();

            var topStatistics = new List<StatisticDisplay>
            {
                new AccuracyStatistic(score.Accuracy),
                new ComboStatistic(score.MaxCombo, beatmapMaxCombo),
                new PerformanceStatistic(score),
            };

            var bottomStatistics = new List<HitResultStatistic>();

            foreach (var result in score.GetStatisticsForDisplay(rulesets))
                bottomStatistics.Add(new HitResultStatistic(result));

            statisticDisplays.AddRange(topStatistics);
            statisticDisplays.AddRange(bottomStatistics);

            AddInternal(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new RomanisableString(metadata.TitleUnicode, metadata.Title),
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                                MaxWidth = ScorePanel.EXPANDED_WIDTH - padding * 2,
                                Truncate = true,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist),
                                Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                                MaxWidth = ScorePanel.EXPANDED_WIDTH - padding * 2,
                                Truncate = true,
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 40 },
                                RelativeSizeAxes = Axes.X,
                                Height = 230,
                                Child = new AccuracyCircle(score, withFlair)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                }
                            },
                            scoreCounter = new TotalScoreCounter(!withFlair)
                            {
                                Margin = new MarginPadding { Top = 0, Bottom = 5 },
                                Current = { Value = 0 },
                                Alpha = 0,
                                AlwaysPresent = true
                            },
                            starAndModDisplay = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(5, 0),
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = beatmap.DifficultyName,
                                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                        MaxWidth = ScorePanel.EXPANDED_WIDTH - padding * 2,
                                        Truncate = true,
                                    },
                                    new OsuTextFlowContainer(s => s.Font = OsuFont.Torus.With(size: 12))
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                    }.With(t =>
                                    {
                                        if (!string.IsNullOrEmpty(creator))
                                        {
                                            t.AddText("mapped by ");
                                            t.AddText(creator, s => s.Font = s.Font.With(weight: FontWeight.SemiBold));
                                        }
                                    })
                                }
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { topStatistics.Cast<Drawable>().ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { bottomStatistics.Where(s => s.Result <= HitResult.Perfect).ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { bottomStatistics.Where(s => s.Result > HitResult.Perfect).ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            }
                        }
                    }
                }
            });

            if (score.Date != default)
                AddInternal(new PlayedOnText(score.Date));

            var starDifficulty = beatmapDifficultyCache.GetDifficultyAsync(beatmap, score.GetRuleset(rulesets), score.GetMods(rulesets)).GetResultSafely();

            if (starDifficulty != null)
            {
                starAndModDisplay.Add(new StarRatingDisplay(starDifficulty.Value)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                });
            }

            var mods = score.GetMods(rulesets);

            if (mods.Any())
            {
                starAndModDisplay.Add(new ModDisplay
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    ExpansionMode = ExpansionMode.AlwaysExpanded,
                    Scale = new Vector2(0.5f),
                    Current = { Value = mods }
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Score counter value setting must be scheduled so it isn't transferred instantaneously
            ScheduleAfterChildren(() =>
            {
                using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DELAY))
                {
                    scoreCounter.FadeIn();
                    scoreCounter.Current = scoreManager.GetBindableTotalScore(score);

                    double delay = 0;

                    foreach (var stat in statisticDisplays)
                    {
                        using (BeginDelayedSequence(delay))
                            stat.Appear();

                        delay += 200;
                    }
                }

                if (!withFlair)
                    FinishTransforms(true);
            });
        }

        public class PlayedOnText : OsuSpriteText
        {
            public PlayedOnText(DateTimeOffset time)
            {
                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
                Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold);
                Text = $"Played on {time.ToLocalTime():d MMMM yyyy HH:mm}";
            }
        }
    }
}
