// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedgeContent : CompositeDrawable
    {
        public OsuSpriteText TitleLabel { get; private set; } = null!;
        public OsuSpriteText ArtistLabel { get; private set; } = null!;

        private readonly WorkingBeatmap working;

        private const float content_margin = SongSelectV2.WEDGE_CONTENT_MARGIN;

        public BeatmapMainWedgeContent(WorkingBeatmap working)
        {
            this.working = working;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(SongSelect? songSelect, LocalisationManager localisation)
        {
            var metadata = working.Metadata;

            var titleText = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            var artistText = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);
            var status = working.BeatmapInfo.Status;

            titleText = new RomanisableString("Exit This Earth's Atomosphere", "Exit This Earth's Atomosphere");
            artistText = new RomanisableString("Camellia", "Camellia");
            status = BeatmapOnlineStatus.Ranked;

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
                            Child = TitleLabel = new TruncatingSpriteText
                            {
                                Shadow = true,
                                Text = titleText,
                                UseFullGlyphHeight = false,
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
                                UseFullGlyphHeight = false,
                                Font = OsuFont.Torus.With(size: 28.8f, weight: FontWeight.SemiBold),
                            },
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(4f, 0f),
                            Margin = new MarginPadding { Top = 6f },
                            Children = new Drawable[]
                            {
                                new BeatmapMainWedgeStatistic(OsuIcon.Play, "3,456,317", BeatmapsetsStrings.ShowStatsPlaycount),
                                new BeatmapMainWedgeStatistic(OsuIcon.Heart, "4,231", BeatmapsStrings.StatusFavourites),
                                new BeatmapMainWedgeStatistic(OsuIcon.Beatmap, "3:45", BeatmapsetsStrings.ShowStatsTotalLength("01:44")),
                                new BeatmapMainWedgeStatistic(OsuIcon.ModDoubleTime, "124", BeatmapsetsStrings.ShowStatsBpm),
                            },
                        },
                    }
                }
            };
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
