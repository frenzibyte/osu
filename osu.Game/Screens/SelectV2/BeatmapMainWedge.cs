// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedge : CompositeDrawable
    {
        private const float transition_duration = 250;
        private const float corner_radius = 10;
        private const float border_weight = 2;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        protected Container? DisplayedContent { get; private set; }

        private BeatmapMainWedgeStatistic playsStatistic = null!;
        private BeatmapMainWedgeStatistic favouritesStatistic = null!;
        private BeatmapMainWedgeStatistic lengthStatistic = null!;
        private BeatmapMainWedgeStatistic bpmStatistic = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private OsuHoverContainer titleLink = null!;
        private OsuSpriteText titleLabel = null!;
        private OsuHoverContainer artistLink = null!;
        private OsuSpriteText artistLabel = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        [Resolved]
        private LocalisationManager localisation { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapCache { get; set; } = null!;

        public BeatmapMainWedge()
        {
            Width = 740f;
            Height = 190;
            Y = -20;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Shear = shear;
            Masking = true;
            Margin = new MarginPadding { Left = -corner_radius - 8 };
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Colour4.Black.Opacity(0.2f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
            };
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.5f),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = -shear,
                    Children = new[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Top = 20 },
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                statusPill = new BeatmapSetOnlineStatusPill
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Right = 20f, Top = 10f },
                                    TextSize = 11,
                                    TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                    // Status = status,
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Bottom = 24 },
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(0f, 10f),
                            Children = new Drawable[]
                            {
                                titleLink = new OsuHoverContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Bottom = -10f },
                                    Child = titleLabel = new TruncatingSpriteText
                                    {
                                        Shadow = true,
                                        Font = OsuFont.TorusAlternate.With(size: 43.2f, weight: FontWeight.SemiBold),
                                    },
                                },
                                artistLink = new OsuHoverContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Left = 1f },
                                    Child = artistLabel = new TruncatingSpriteText
                                    {
                                        Shadow = true,
                                        Font = OsuFont.Torus.With(size: 28.8f, weight: FontWeight.SemiBold),
                                    },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(2f, 0f),
                                    AutoSizeDuration = 100,
                                    AutoSizeEasing = Easing.OutQuint,
                                    Children = new Drawable[]
                                    {
                                        playsStatistic = new BeatmapMainWedgeStatistic(OsuIcon.Play, background: true, leftPadding: SongSelect.WEDGE_CONTENT_MARGIN)
                                        {
                                            TooltipText = BeatmapsetsStrings.ShowStatsPlaycount,
                                            Margin = new MarginPadding { Left = -SongSelect.WEDGE_CONTENT_MARGIN },
                                        },
                                        favouritesStatistic = new BeatmapMainWedgeStatistic(OsuIcon.Heart, background: true)
                                        {
                                            TooltipText = BeatmapsStrings.StatusFavourites,
                                        },
                                        lengthStatistic = new BeatmapMainWedgeStatistic(OsuIcon.Clock)
                                        {
                                        },
                                        bpmStatistic = new BeatmapMainWedgeStatistic(OsuIcon.BPM)
                                        {
                                            TooltipText = BeatmapsetsStrings.ShowStatsBpm,
                                        },
                                    },
                                },
                            }
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // displayedStars.BindValueChanged(s =>
            // {
            //     difficultyBorder.Colour = colours.ForStarDifficulty(s.NewValue);
            // }, true);

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            mods.BindValueChanged(_ => updateDisplay());
            updateDisplay();

            FinishTransforms(true);

            this.MoveToX(-150)
                .MoveToX(0, SongSelect.ENTER_DURATION, Easing.OutQuint)
                .FadeInFromZero(SongSelect.ENTER_DURATION / 3, Easing.In);
        }

        private int? currentBeatmapSetID;

        private void updateDisplay()
        {
            var metadata = beatmap.Value.Metadata;
            var beatmapInfo = beatmap.Value.BeatmapInfo;

            statusPill.Status = beatmapInfo.Status;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            titleLabel.Text = titleText;
            titleLink.Action = () => songSelect?.Search(titleText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            artistLabel.Text = artistText;
            artistLink.Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript));

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            int bpmMax = FormatUtils.RoundBPM(beatmap.Value.Beatmap.ControlPointInfo.BPMMaximum, rate);
            int bpmMin = FormatUtils.RoundBPM(beatmap.Value.Beatmap.ControlPointInfo.BPMMinimum, rate);
            int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.Value.Beatmap.GetMostCommonBeatLength(), rate);

            double drainLength = Math.Round(beatmap.Value.Beatmap.CalculateDrainLength() / rate);
            double hitLength = Math.Round(beatmapInfo.Length / rate);

            lengthStatistic.Value = hitLength.ToFormattedDuration();
            lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

            bpmStatistic.Value = bpmMin == bpmMax
                ? $"{bpmMin}"
                : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})";

            updateOnlineDisplay();
        }

        private void updateOnlineDisplay()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            var beatmapSetInfo = beatmap.Value.BeatmapSetInfo;

            if (currentBeatmapSetID == null || currentBeatmapSetID != beatmapSetInfo.OnlineID)
            {
                int? firstOnlineID = beatmapSetInfo.Beatmaps.FirstOrDefault(b => b.OnlineID >= 1)?.OnlineID;

                if (firstOnlineID != null)
                {
                    playsStatistic.FadeIn(300, Easing.OutQuint);
                    playsStatistic.Value = null;

                    favouritesStatistic.FadeIn(300, Easing.OutQuint);
                    favouritesStatistic.Value = null;

                    var token = cancellationSource.Token;

                    beatmapCache.GetBeatmapAsync(firstOnlineID.Value, token).ContinueWith(t => Schedule(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        var apiBeatmap = t.GetResultSafely();

                        if (apiBeatmap != null)
                        {
                            playsStatistic.Value = apiBeatmap.BeatmapSet!.PlayCount.ToLocalisableString(@"N0");
                            favouritesStatistic.Value = apiBeatmap.BeatmapSet!.FavouriteCount.ToLocalisableString(@"N0");
                        }

                        currentBeatmapSetID = beatmapSetInfo.OnlineID;
                    }), token);
                }
                else
                {
                    playsStatistic.FadeOut(300, Easing.OutQuint);
                    favouritesStatistic.FadeOut(300, Easing.OutQuint);
                    currentBeatmapSetID = null;
                }
            }
        }
    }
}
