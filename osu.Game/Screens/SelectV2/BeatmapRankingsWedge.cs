// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using LeaderboardScoreV2 = osu.Game.Screens.SelectV2.Leaderboards.LeaderboardScoreV2;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapRankingsWedge : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private Container scores = null!;

        private OsuScrollContainer scoresScroll = null!;
        private Container personalBestContainer = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding { Top = -10f };

            InternalChildren = new Drawable[]
            {
                scoresScroll = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Padding = new MarginPadding { Left = 80 },
                    Shear = shear,
                    Child = scores = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = 14f, Bottom = 20f },
                    },
                },
                personalBestContainer = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Shear = shear,
                    Margin = new MarginPadding { Left = -60f },
                    CornerRadius = 16f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -shear,
                            Padding = new MarginPadding { Top = 5f, Bottom = 30f, Left = 100f, Right = 30f },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = colourProvider.Content2,
                                    Text = "Personal Best",
                                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                                },
                                new LeaderboardScoreV2(new ScoreInfo
                                {
                                    Position = 999,
                                    Rank = ScoreRank.X,
                                    Accuracy = 1,
                                    MaxCombo = 244,
                                    TotalScore = RNG.Next(1_800_000, 2_000_000),
                                    MaximumStatistics = { { HitResult.Great, 3000 } },
                                    Ruleset = rulesets.GetRuleset(0)!,
                                    User = new APIUser
                                    {
                                        Id = 6602580,
                                        Username = @"waaiiru",
                                        CountryCode = CountryCode.ES,
                                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                                    },
                                    Date = DateTimeOffset.Now.AddYears(-2),
                                    APIMods = new[]
                                    {
                                        new APIMod { Acronym = "DT" },
                                        new APIMod { Acronym = "HD" },
                                        new APIMod { Acronym = "HR" },
                                        new APIMod { Acronym = "FL" },
                                        new APIMod { Acronym = "DC" },
                                    }
                                })
                                {
                                    Margin = new MarginPadding { Top = 20f },
                                },
                            }
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoresScroll.Padding = scoresScroll.Padding with { Bottom = 100f };

            foreach (var score in scores)
            {
                score.MoveToX(-100f, 300, Easing.OutQuint);
                score.FadeOut(300, Easing.OutQuint);
                score.Expire();
            }

            int delay = 100;
            int accumulation = 1;

            for (int i = 0; i < 50; i++)
            {
                var score = new ScoreInfo
                {
                    Position = 999,
                    Rank = ScoreRank.X,
                    Accuracy = 1,
                    MaxCombo = 244,
                    TotalScore = RNG.Next(1_800_000, 2_000_000),
                    MaximumStatistics = { { HitResult.Great, 3000 } },
                    Ruleset = rulesets.GetRuleset(0)!,
                    User = new APIUser
                    {
                        Id = 6602580,
                        Username = @"waaiiru",
                        CountryCode = CountryCode.ES,
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    },
                    Date = DateTimeOffset.Now.AddYears(-2),
                    APIMods = new[]
                    {
                        new APIMod { Acronym = "DT" },
                        new APIMod { Acronym = "HD" },
                        new APIMod { Acronym = "HR" },
                        new APIMod { Acronym = "FL" },
                        new APIMod { Acronym = "DC" },
                    }
                };

                Container drawable;

                scores.Add(drawable = new Container
                {
                    Shear = new Vector2(-OsuGame.SHEAR, 0),
                    Y = (LeaderboardScoreV2.HEIGHT + 4f) * i,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Alpha = 0f,
                    Child = new LeaderboardScoreV2(score),
                });

                drawable.Delay(delay).FadeIn(300, Easing.OutQuint);
                drawable.MoveToX(-100f).Delay(delay).MoveToX(0f, 300, Easing.OutQuint);

                delay += Math.Max(0, 50 - accumulation);
                accumulation *= 2;
            }
        }
    }
}
