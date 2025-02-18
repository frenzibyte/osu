// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2.Leaderboards
{
    public partial class LeaderboardScoreV2Tooltip : VisibilityContainer, ITooltip<ScoreInfo>
    {
        private const float spacing = 20f;

        private DateAndStatisticsPanel dateAndStatistics = null!;
        private ModsPanel modsPanel = null!;
        private MultiplierPanel multiplierPanel = null!;
        private TotalScoreRankPanel totalScoreRankPanel = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider;

        public LeaderboardScoreV2Tooltip(OverlayColourProvider colourProvider)
        {
            this.colourProvider = colourProvider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = 180;
            AutoSizeAxes = Axes.Y;

            InternalChild = new ReverseChildIDFillFlowContainer<Drawable>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0f, -spacing),
                Children = new Drawable[]
                {
                    dateAndStatistics = new DateAndStatisticsPanel(),
                    modsPanel = new ModsPanel(),
                    multiplierPanel = new MultiplierPanel(),
                    totalScoreRankPanel = new TotalScoreRankPanel(),
                },
            };
        }

        private ScoreInfo? lastContent;

        public void SetContent(ScoreInfo content)
        {
            if (lastContent != null && lastContent.Equals(content))
                return;

            dateAndStatistics.Score = content;
            modsPanel.Score = content;
            multiplierPanel.Score = content;
            totalScoreRankPanel.Score = content;
            lastContent = content;
        }

        protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
        public void Move(Vector2 pos) => Position = pos;

        private partial class DateAndStatisticsPanel : CompositeDrawable
        {
            public OsuSpriteText AbsoluteDate = null!;
            public DrawableDate RelativeDate = null!;
            public FillFlowContainer Statistics = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ScoreInfo Score
            {
                set
                {
                    AbsoluteDate.Text = value.Date.ToLocalisableString(@"dd MMMM yyyy h:mm tt");
                    RelativeDate.Date = value.Date;

                    var judgementsStatistics = value.GetStatisticsForDisplay().Select(s =>
                    {
                        Colour4 colour = colours.ForHitResult(s.Result);
                        var hsl = colour.ToHSL();

                        Colour4 lightColour = Colour4.FromHSL(hsl.X, hsl.Y, 0.8f);
                        return new StatisticRow(s.DisplayName, lightColour, s.Count.ToLocalisableString("N0"));
                    });

                    var generalStatistics = new[]
                    {
                        new StatisticRow("Max Combo", colourProvider.Content2, value.MaxCombo.ToLocalisableString(@"0\x")),
                        new StatisticRow("Accuracy", colourProvider.Content2, value.Accuracy.FormatAccuracy()),
                    };

                    if (value.PP != null)
                    {
                        generalStatistics = new[]
                        {
                            new StatisticRow("PP", colourProvider.Content2, value.PP.ToLocalisableString("N0"))
                        }.Concat(generalStatistics).ToArray();
                    }

                    Statistics.ChildrenEnumerable = judgementsStatistics
                                                    .Append(Empty().With(d => d.Height = 25))
                                                    .Concat(generalStatistics);
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius = 10;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4f,
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, 5f),
                        Margin = new MarginPadding { Top = 10f },
                        Children = new Drawable[]
                        {
                            AbsoluteDate = new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                                UseFullGlyphHeight = false,
                            },
                            RelativeDate = new DrawableDate(new DateTime(2024, 2, 3, 12, 45, 0), 14.4f, weight: FontWeight.SemiBold)
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Colour = colourProvider.Content2,
                                UseFullGlyphHeight = false,
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                CornerRadius = 10,
                                Masking = true,
                                Margin = new MarginPadding { Top = 5f },
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background3,
                                    },
                                    Statistics = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(0f, 5f),
                                        Padding = new MarginPadding { Horizontal = 10f, Vertical = 10f },
                                    },
                                },
                            },
                        },
                    },
                };
            }
        }

        private partial class StatisticRow : CompositeDrawable
        {
            public StatisticRow(LocalisableString label, Color4 labelColour, LocalisableString value)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new[]
                {
                    new OsuSpriteText
                    {
                        Text = label,
                        Colour = labelColour,
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Text = value,
                        Colour = Color4.White,
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.Bold),
                    },
                };
            }
        }

        private partial class ModsPanel : CompositeDrawable
        {
            public FillFlowContainer Mods = null!;

            public ScoreInfo Score
            {
                set => Mods.ChildrenEnumerable = value.Mods.Select(m => new LeaderboardScoreV2.ColouredModSwitchTiny(m)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.375f),
                });
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, OsuColour colours)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius = 10;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4f,
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Box
                    {
                        Colour = ColourInfo.GradientVertical(colours.Red1.Opacity(0f), colours.Red1.Opacity(0.25f)),
                        RelativeSizeAxes = Axes.Both,
                    },
                    Mods = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Bottom = 8f, Top = 8f + spacing },
                        Padding = new MarginPadding { Horizontal = 20f },
                        Spacing = new Vector2(3f),
                    },
                };
            }
        }

        private partial class MultiplierPanel : CompositeDrawable
        {
            public Box Background = null!;
            public Box ValueBackground = null!;
            public OsuSpriteText Label = null!;
            public OsuSpriteText Value = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public ScoreInfo Score
            {
                set
                {
                    double multiplier = 1.0;

                    foreach (var mod in value.Mods)
                        multiplier *= mod.ScoreMultiplier;

                    Color4 light = multiplier > 1 ? colours.Red1 : colours.Lime1;
                    Color4 dark = multiplier > 1 ? colours.Red4 : colours.Lime4;

                    Background.Colour = light;
                    ValueBackground.Colour = dark;
                    Label.Colour = dark;
                    Value.Text = ModUtils.FormatScoreMultiplier(multiplier);
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius = 10;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4f,
                };

                InternalChildren = new Drawable[]
                {
                    Background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Red1,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Bottom = 4f, Top = 4f + spacing },
                        Padding = new MarginPadding { Horizontal = 10f },
                        Children = new Drawable[]
                        {
                            Label = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "Multiplier",
                                Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                                Colour = colours.Red4,
                            },
                            new CircularContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Masking = true,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    ValueBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Red4,
                                    },
                                    Value = new OsuSpriteText
                                    {
                                        Text = "1.97x",
                                        Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                                        Margin = new MarginPadding { Horizontal = 13f, Vertical = 4f },
                                        UseFullGlyphHeight = false,
                                    }
                                },
                            },
                        }
                    }
                };
            }
        }

        public partial class TotalScoreRankPanel : CompositeDrawable
        {
            public Box RankBackground = null!;
            public UpdateableRank Rank = null!;
            public OsuSpriteText TotalScore = null!;

            public ScoreInfo Score
            {
                set
                {
                    RankBackground.Colour = ColourInfo.GradientVertical(
                        OsuColour.ForRank(value.Rank).Opacity(0f),
                        OsuColour.ForRank(value.Rank).Opacity(0.5f));
                    Rank.Rank = value.Rank;
                    TotalScore.Text = value.TotalScore.ToLocalisableString("N0");
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                CornerRadius = 10;
                Masking = true;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4f,
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#353535"),
                    },
                    RankBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    Rank = new UpdateableRank
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(32f, 16f),
                        Margin = new MarginPadding { Bottom = 8f },
                    },
                    TotalScore = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Text = "9,999,999",
                        Margin = new MarginPadding { Bottom = 24f + 8f, Top = 8f + spacing },
                        Font = OsuFont.Torus.With(size: 38.4f, weight: FontWeight.Light),
                        UseFullGlyphHeight = false,
                    },
                };
            }
        }
    }
}
