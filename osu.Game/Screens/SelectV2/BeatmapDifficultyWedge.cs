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
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
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
        private OsuSpriteText mapperText = null!;
        private MapperLinkContainer mapperLink = null!;

        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> beatmapStatisticsFlow = null!;
        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> difficultyStatisticsFlow = null!;

        private BeatmapDifficultyWedgeStatistic firstDifficultyStatistic = null!;
        private BeatmapDifficultyWedgeStatistic accuracyStatistic = null!;
        private BeatmapDifficultyWedgeStatistic hpDrainStatistic = null!;
        private BeatmapDifficultyWedgeStatistic approachRateStatistic = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public BeatmapDifficultyWedge()
        {
            Width = 650;
            Height = 80;
            X = -18;
            Y = 172;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10;
            Shear = shear;

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
                    Margin = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
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
                            Margin = new MarginPadding { Bottom = 2f },
                            Children = new Drawable[]
                            {
                                difficultyText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = OsuFont.Torus.With(size: 19.2f, weight: FontWeight.SemiBold),
                                    Colour = Color4.Black.Opacity(0.75f),
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = " mapped by ",
                                    Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.Regular),
                                    Colour = Color4.Black.Opacity(0.75f),
                                },
                                mapperLink = new MapperLinkContainer
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Child = mapperText = new OsuSpriteText
                                    {
                                        Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.SemiBold),
                                        Colour = Color4.Black.Opacity(0.75f),
                                    },
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
                                Margin = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN + 6, Top = 7.5f },
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
                                            firstDifficultyStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsCs),
                                            accuracyStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsAccuracy),
                                            hpDrainStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsDrain),
                                            approachRateStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsAr),
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
            FinishTransforms(true);
        }

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        private void updateDisplay()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            computeStarDifficulty(cancellationSource.Token);

            difficultyText.Text = beatmap.Value.BeatmapInfo.DifficultyName;
            mapperText.Text = beatmap.Value.Metadata.Author.Username;
            mapperLink.Action = () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, beatmap.Value.Metadata.Author));

            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(ruleset.Value);
            beatmapStatisticsFlow.Children = playableBeatmap.GetStatistics().Select(s => new BeatmapDifficultyWedgeStatistic(s.Name)
            {
                Value = (int.Parse(s.Content), 1f),
            }).ToArray();

            BeatmapDifficulty? baseDifficulty = beatmap.Value.BeatmapInfo.Difficulty;

            if (baseDifficulty != null)
            {
                BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(originalDifficulty);

                var rateAdjustedDifficulty = originalDifficulty;

                if (ruleset.Value != null)
                {
                    double rate = ModUtils.CalculateRateWithMods(mods.Value);

                    rateAdjustedDifficulty = ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

                    // TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, adjustedDifficulty);
                }

                switch (ruleset.Value?.OnlineID)
                {
                    case 3:
                        // Account for mania differences locally for now.
                        // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes.
                        ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();

                        // For the time being, the key count is static no matter what, because:
                        // a) The method doesn't have knowledge of the active keymods. Doing so may require considerations for filtering.
                        // b) Using the difficulty adjustment mod to adjust OD doesn't have an effect on conversion.
                        int keyCount = legacyRuleset.GetKeyCount(beatmap.Value.BeatmapInfo, mods.Value);

                        firstDifficultyStatistic.Label = BeatmapsetsStrings.ShowStatsCsMania;
                        firstDifficultyStatistic.Value = (keyCount, 10);
                        break;

                    default:
                        firstDifficultyStatistic.Label = BeatmapsetsStrings.ShowStatsCs;
                        firstDifficultyStatistic.Value = (rateAdjustedDifficulty.CircleSize, 10f);
                        break;
                }

                accuracyStatistic.Value = (rateAdjustedDifficulty.OverallDifficulty, 10f);
                hpDrainStatistic.Value = (rateAdjustedDifficulty.DrainRate, 10f);
                approachRateStatistic.Value = (rateAdjustedDifficulty.ApproachRate, 10f);
            }
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

        private partial class MapperLinkContainer : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }
    }
}
