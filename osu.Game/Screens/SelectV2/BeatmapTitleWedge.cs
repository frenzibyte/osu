// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge : VisibilityContainer
    {
        private const float corner_radius = 10;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        protected override bool StartHidden => true;

        private ModSettingChangeTracker? settingChangeTracker;

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container titleContainer = null!;
        private OsuHoverContainer titleLink = null!;
        private OsuSpriteText titleLabel = null!;
        private Container artistContainer = null!;
        private OsuHoverContainer artistLink = null!;
        private OsuSpriteText artistLabel = null!;

        internal string DisplayedTitle => titleLabel.Text.ToString();
        internal string DisplayedArtist => artistLabel.Text.ToString();

        private StatisticPlayCount playCount = null!;

        private Statistic favouritesStatistic = null!;
        // private Statistic lengthStatistic = null!;
        // private Statistic bpmStatistic = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private LocalisationManager localisation { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private APIBeatmapSet? currentOnlineBeatmapSet;
        private GetBeatmapSetRequest? currentRequest;

        // private FillFlowContainer statisticsFlow = null!;

        public BeatmapTitleWedge()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                new WedgeBackground(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding
                    {
                        Top = SongSelect.WEDGE_CONTENT_MARGIN,
                        Left = SongSelect.WEDGE_CONTENT_MARGIN
                    },
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new ShearAligningWrapper(new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = OsuFont.Style.Heading2.Size,
                            Shear = -OsuGame.SHEAR,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(4f, 0f),
                                        Children = new Drawable[]
                                        {
                                            statusPill = new BeatmapSetOnlineStatusPill
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                ShowUnknownStatus = true,
                                                TextSize = OsuFont.Style.Caption1.Size,
                                                TextPadding = new MarginPadding { Horizontal = 6, Vertical = 1 },
                                            },
                                            titleContainer = new Container
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                AutoSizeAxes = Axes.X,
                                                Margin = new MarginPadding { Bottom = 1, Left = 4 },
                                                Height = OsuFont.Style.Heading2.Size,
                                                Child = titleLink = new OsuHoverContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Child = titleLabel = new TruncatingSpriteText
                                                    {
                                                        Shadow = true,
                                                        Font = OsuFont.Style.Heading2.With(weight: FontWeight.SemiBold),
                                                        MaxWidth = 200,
                                                    },
                                                }
                                            },
                                            new OsuSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Text = "-",
                                            },
                                            artistContainer = new Container
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                AutoSizeAxes = Axes.X,
                                                Height = OsuFont.Style.Heading2.Size,
                                                Margin = new MarginPadding { Bottom = 1 },
                                                Child = artistLink = new OsuHoverContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Child = artistLabel = new TruncatingSpriteText
                                                    {
                                                        Shadow = true,
                                                        Font = OsuFont.Style.Heading2,
                                                        MaxWidth = 150,
                                                    },
                                                }
                                            },
                                            // lengthStatistic = new Statistic(OsuIcon.Clock)
                                            // {
                                            //     Anchor = Anchor.CentreLeft,
                                            //     Origin = Anchor.CentreLeft,
                                            //     Scale = new Vector2(OsuFont.Style.Body.Size / OsuFont.Style.Heading2.Size),
                                            // },
                                            // bpmStatistic = new Statistic(OsuIcon.Metronome)
                                            // {
                                            //     Anchor = Anchor.CentreLeft,
                                            //     Origin = Anchor.CentreLeft,
                                            //     TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                            //     Margin = new MarginPadding { Left = 5f },
                                            //     Scale = new Vector2(OsuFont.Style.Body.Size / OsuFont.Style.Heading2.Size),
                                            // },
                                        },
                                    },
                                    Empty(),
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.X,
                                        RelativeSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(4f, 0f),
                                        Children = new[]
                                        {
                                            favouritesStatistic = new Statistic(OsuIcon.Heart, background: true, minSize: 25f)
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                TooltipText = BeatmapsStrings.StatusFavourites,
                                                Margin = new MarginPadding { Right = 10 },
                                                Scale = new Vector2(OsuFont.Style.Body.Size / OsuFont.Style.Heading2.Size),
                                            },
                                            playCount = new StatisticPlayCount(background: true, minSize: 50f)
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Scale = new Vector2(OsuFont.Style.Body.Size / OsuFont.Style.Heading2.Size),
                                            },
                                        }
                                    }
                                },
                            }
                        }),
                        new ShearAligningWrapper(new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Padding = new MarginPadding { Right = -SongSelect.WEDGE_CONTENT_MARGIN },
                            Child = new DifficultyDisplay(),
                        }),
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            working.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());

            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                updateLengthAndBpmStatistics();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => updateLengthAndBpmStatistics();
            });

            updateDisplay();

            // statisticsFlow.AutoSizeDuration = 100;
            // statisticsFlow.AutoSizeEasing = Easing.OutQuint;
        }

        protected override void PopIn()
        {
            this.MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(-150, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeOut(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        protected override void Update()
        {
            base.Update();
            // titleLabel.MaxWidth = titleContainer.DrawWidth - 20;
            // artistLabel.MaxWidth = artistContainer.DrawWidth - 20;
        }

        private void updateDisplay()
        {
            var metadata = working.Value.Metadata;
            var beatmapInfo = working.Value.BeatmapInfo;
            var beatmapSetInfo = working.Value.BeatmapSetInfo;

            statusPill.Status = beatmapInfo.Status;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            titleLabel.Text = titleText;
            titleLink.Action = () => songSelect?.Search(titleText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            artistLabel.Text = artistText;
            artistLink.Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            updateLengthAndBpmStatistics();

            if (currentOnlineBeatmapSet == null || currentOnlineBeatmapSet.OnlineID != beatmapSetInfo.OnlineID)
                refetchBeatmapSet();

            updateOnlineDisplay();
        }

        private CancellationTokenSource? lengthBpmCancellationSource;

        private void updateLengthAndBpmStatistics()
        {
            lengthBpmCancellationSource?.Cancel();
            lengthBpmCancellationSource = new CancellationTokenSource();

            var token = lengthBpmCancellationSource.Token;

            Task.Run(() =>
            {
                Schedule(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    // lengthStatistic.Text = hitLength.ToFormattedDuration();
                    // lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());
                    //
                    // bpmStatistic.Text = bpmMin == bpmMax
                    //     ? $"{bpmMin}"
                    //     : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";
                });
            }, token);
        }

        private void refetchBeatmapSet()
        {
            var beatmapSetInfo = working.Value.BeatmapSetInfo;

            currentRequest?.Cancel();
            currentRequest = null;
            currentOnlineBeatmapSet = null;

            if (beatmapSetInfo.OnlineID >= 1)
            {
                // todo: consider introducing a BeatmapSetLookupCache for caching benefits.
                currentRequest = new GetBeatmapSetRequest(beatmapSetInfo.OnlineID);
                currentRequest.Failure += _ => updateOnlineDisplay();
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
                playCount.Value = null;
                favouritesStatistic.Text = null;
            }
            else if (currentOnlineBeatmapSet == null)
            {
                playCount.Value = new StatisticPlayCount.Data(-1, -1);
                favouritesStatistic.Text = "-";
            }
            else
            {
                var onlineBeatmapSet = currentOnlineBeatmapSet;
                var onlineBeatmap = currentOnlineBeatmapSet.Beatmaps.SingleOrDefault(b => b.OnlineID == working.Value.BeatmapInfo.OnlineID);

                playCount.Value = new StatisticPlayCount.Data(onlineBeatmap?.PlayCount ?? -1, onlineBeatmap?.UserPlayCount ?? -1);
                favouritesStatistic.Text = onlineBeatmapSet.FavouriteCount.ToLocalisableString(@"N0");
            }
        }
    }
}
