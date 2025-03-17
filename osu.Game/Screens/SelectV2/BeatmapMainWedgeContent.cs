// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedgeContent : CompositeDrawable
    {
        public OsuSpriteText TitleLabel { get; private set; } = null!;
        public OsuSpriteText ArtistLabel { get; private set; } = null!;

        private readonly WorkingBeatmap working;
        private readonly IReadOnlyList<Mod> mods;

        private BeatmapMainWedgeStatistic playsStatistic = null!;
        private BeatmapMainWedgeStatistic favouritesStatistic = null!;

        private const float content_margin = SongSelect.WEDGE_CONTENT_MARGIN;

        public BeatmapMainWedgeContent(WorkingBeatmap working, IReadOnlyList<Mod> mods)
        {
            this.working = working;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(SongSelect? songSelect, LocalisationManager localisation)
        {
            var metadata = working.Metadata;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            var status = working.BeatmapInfo.Status;

            double rate = ModUtils.CalculateRateWithMods(mods);

            int bpmMax = FormatUtils.RoundBPM(working.Beatmap.ControlPointInfo.BPMMaximum, rate);
            int bpmMin = FormatUtils.RoundBPM(working.Beatmap.ControlPointInfo.BPMMinimum, rate);
            int mostCommonBPM = FormatUtils.RoundBPM(60000 / working.Beatmap.GetMostCommonBeatLength(), rate);

            double drainLength = Math.Round(working.Beatmap.CalculateDrainLength() / rate);
            double hitLength = Math.Round(working.Beatmap.BeatmapInfo.Length / rate);

            InternalChildren = new[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding { Left = content_margin, Top = 24 },
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new BeatmapSetOnlineStatusPill
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Right = 20f, Top = 10f },
                            TextSize = 11,
                            TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                            Status = status,
                        },
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = content_margin, Bottom = 24 },
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new OsuHoverContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Action = () => songSelect?.Search(titleText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript)),
                            Margin = new MarginPadding { Bottom = -10f },
                            Child = TitleLabel = new TruncatingSpriteText
                            {
                                Shadow = true,
                                Text = titleText,
                                Font = OsuFont.TorusAlternate.With(size: 48, weight: FontWeight.SemiBold),
                            },
                        },
                        new OsuHoverContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript)),
                            Margin = new MarginPadding { Left = 1f },
                            Child = ArtistLabel = new TruncatingSpriteText
                            {
                                Shadow = true,
                                Text = artistText,
                                Font = OsuFont.Torus.With(size: 28.8f, weight: FontWeight.SemiBold),
                            },
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(4f, 0f),
                            Children = new Drawable[]
                            {
                                playsStatistic = new BeatmapMainWedgeStatistic(OsuIcon.Play, string.Empty, BeatmapsetsStrings.ShowStatsPlaycount),
                                favouritesStatistic = new BeatmapMainWedgeStatistic(OsuIcon.Heart, string.Empty, BeatmapsStrings.StatusFavourites),
                                new BeatmapMainWedgeStatistic(OsuIcon.Clock,
                                    hitLength.ToFormattedDuration(),
                                    BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration())),
                                new BeatmapMainWedgeStatistic(OsuIcon.BPM,
                                    bpmMin == bpmMax ? $"{bpmMin}" : $"{bpmMin}-{bpmMax} (mostly {mostCommonBPM})",
                                    BeatmapsetsStrings.ShowStatsBpm),
                            },
                        },
                    }
                }
            };
        }

        [Resolved]
        private BeatmapLookupCache beatmapCache { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapCache.GetBeatmapAsync(working.BeatmapInfo.OnlineID).ContinueWith(t => Schedule(() =>
            {
                var beatmap = t.GetResultSafely();

                if (beatmap != null)
                {
                    playsStatistic.Value = beatmap.BeatmapSet!.PlayCount.ToLocalisableString(@"N0");
                    favouritesStatistic.Value = beatmap.BeatmapSet!.FavouriteCount.ToLocalisableString(@"N0");
                }
            }));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // best effort to confine the auto-sized text to wedge bounds
            // the artist label doesn't have an extra text_margin as it doesn't touch the right metadata
            float shearWidth = OsuGame.SHEAR * DrawHeight;

            TitleLabel.MaxWidth = DrawWidth - content_margin * 2 - shearWidth;
            ArtistLabel.MaxWidth = DrawWidth - content_margin - shearWidth;
        }
    }
}
