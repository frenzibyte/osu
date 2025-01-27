// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2.Leaderboards
{
    public partial class LeaderboardSection : CompositeDrawable
    {
        private const float corner_radius = 10;

        private FillFlowContainer<LeaderboardScoreV2> scoreFlow = null!;
        private BeatmapLeaderboard leaderboard = null!;

        private Container placeholderContainer = null!;
        private LoadingLayer loading = null!;

        private readonly Bindable<BeatmapLeaderboardScope> scope = new Bindable<BeatmapLeaderboardScope>();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        public LeaderboardSection()
        {
            Width = 700;
            Height = 310;
            Y = 400;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = corner_radius;
            Shear = shear;
            Margin = new MarginPadding { Left = -corner_radius };

            AddInternal(leaderboard = new BeatmapLeaderboard());
            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10f,
                Colour = colourProvider.Background5,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 8f,
                    Colour = Color4.Black.Opacity(0.125f),
                    Offset = new Vector2(0f, 4f),
                },
                Child = new Box { RelativeSizeAxes = Axes.Both },
            });
            AddInternal(new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new LeaderboardSectionHeader
                        {
                            RelativeSizeAxes = Axes.X,
                            Scope = { BindTarget = scope },
                        },
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 10f,
                                    Colour = colourProvider.Background4,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Radius = 8f,
                                        Colour = Color4.Black.Opacity(0.125f),
                                        Offset = new Vector2(0f, 4f),
                                    },
                                    Child = new Box { RelativeSizeAxes = Axes.Both },
                                },
                                new OsuScrollContainer(Direction.Vertical)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarVisible = false,
                                    Child = scoreFlow = new FillFlowContainer<LeaderboardScoreV2>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding
                                        {
                                            Left = 100f,
                                            Right = 8f,
                                            Vertical = 8f,
                                        },
                                        Spacing = new Vector2(0f, 2f),
                                    }
                                },
                                placeholderContainer = new Container
                                {
                                    Shear = -shear,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                loading = new LoadingLayer
                                {
                                    Shear = -shear,
                                },
                            }
                        },
                    }
                },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scope.BindValueChanged(_ => refreshLeaderboard());
            beatmap.BindValueChanged(_ => refreshLeaderboard());
            ruleset.BindValueChanged(_ => refreshLeaderboard());
            mods.BindValueChanged(_ => refreshLeaderboard());
            refreshLeaderboard();

            leaderboard.StateChanged += leaderboardStateChanged;
            leaderboardStateChanged(leaderboard.State);
        }

        private void refreshLeaderboard()
        {
            // todo: missing "filter mod" toggle here.
            leaderboard.FilterMods = false;
            leaderboard.Scope = scope.Value;
            leaderboard.BeatmapInfo = beatmap.Value?.BeatmapInfo;
            leaderboard.RefetchScores();
        }

        private void leaderboardStateChanged(LeaderboardState state) => Scheduler.AddOnce(() =>
        {
            scoreFlow.FadeOut(200, Easing.OutQuint);

            if (state == LeaderboardState.Retrieving)
            {
                loading.Show();
                return;
            }

            loading.Hide();

            var currentPlaceholder = placeholderContainer.SingleOrDefault();
            currentPlaceholder?.FadeOut(150, Easing.OutQuint).Expire();

            var placeholder = getPlaceholderFor(state);

            if (placeholder == null)
            {
                Debug.Assert(state == LeaderboardState.Success);

                scoresFetched();
                scoreFlow.FadeIn(200, Easing.OutQuint);
                return;
            }

            placeholderContainer.Child = placeholder;

            placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 900, Easing.OutQuint);
            placeholder.FadeInFromZero(300, Easing.OutQuint);
        });

        private void scoresFetched()
        {
            var scores = leaderboard.Scores.Select(s => new LeaderboardScoreV2(s)
            {
                Shear = Vector2.Zero,
            });

            LoadComponentsAsync(scores, loaded => scoreFlow.AddRange(loaded));
        }

        private Placeholder? getPlaceholderFor(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NetworkFailure:
                    return new ClickablePlaceholder(LeaderboardStrings.CouldntFetchScores, FontAwesome.Solid.Sync)
                    {
                        Action = refreshLeaderboard
                    };

                case LeaderboardState.NoneSelected:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseSelectABeatmap);

                case LeaderboardState.RulesetUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisRuleset);

                case LeaderboardState.BeatmapUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisBeatmap);

                case LeaderboardState.NoScores:
                    return new MessagePlaceholder(LeaderboardStrings.NoRecordsYet);

                case LeaderboardState.NotLoggedIn:
                    return new LoginPlaceholder(LeaderboardStrings.PleaseSignInToViewOnlineLeaderboards);

                case LeaderboardState.NotSupporter:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard);

                case LeaderboardState.Retrieving:
                    return null;

                case LeaderboardState.Success:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }
    }
}
