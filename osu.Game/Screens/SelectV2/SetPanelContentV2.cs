// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select.Carousel;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class SetPanelContentV2 : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private readonly BeatmapSetInfo setInfo;

        private IBindable<StarDifficulty?> starDifficultyBindable = null!;
        private StarRatingDisplay? starRatingDisplay;
        private FillFlowContainer fillFlow = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public SetPanelContentV2(BeatmapSetInfo setInfo)
        {
            this.setInfo = setInfo;

            // required to ensure we load as soon as any part of the panel comes on screen
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                // required to ensure we load as soon as any part of the panel comes on screen
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding { Top = 5, Left = 18, Right = 10, Bottom = 10 },
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(setInfo.Metadata.TitleUnicode, setInfo.Metadata.Title),
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                        Shadow = true,
                        Shear = -shear,
                    },
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(setInfo.Metadata.ArtistUnicode, setInfo.Metadata.Artist),
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                        Shadow = true,
                        Shear = -shear,
                    },
                    fillFlow = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Top = 5 },
                        Spacing = new Vector2(5),
                        Shear = -shear,
                        Children = new[]
                        {
                            setInfo.AllBeatmapsUpToDate
                                ? Empty().With(d => d.Expire())
                                : new Container
                                {
                                    AutoSizeAxes = Axes.X,
                                    RelativeSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new UpdateBeatmapSetButton(setInfo),
                                    }
                                },
                            new BeatmapSetOnlineStatusPill
                            {
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                Status = setInfo.Status
                            },
                        },
                    }
                }
            };

            if (setInfo.Beatmaps.Count == 1)
            {
                var singleBeatmap = setInfo.Beatmaps.Single();
                fillFlow.AddRange(new Drawable[]
                {
                    starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                    },
                    new TopLocalRank(singleBeatmap)
                    {
                        Scale = new Vector2(8f / 11),
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = singleBeatmap.DifficultyName,
                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                Origin = Anchor.BottomLeft,
                                Anchor = Anchor.BottomLeft,
                            },
                            new OsuSpriteText
                            {
                                Colour = colourProvider.Content2,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                Text = BeatmapsetsStrings.ShowDetailsMappedBy(singleBeatmap.Metadata.Author.Username),
                                Origin = Anchor.BottomLeft,
                                Anchor = Anchor.BottomLeft,
                            }
                        }
                    }
                });
            }
            else
            {
                fillFlow.Add(new DifficultySpectrumDisplay(setInfo)
                {
                    DotSize = new Vector2(5, 10),
                    DotSpacing = 2,
                    Origin = Anchor.CentreLeft,
                    Anchor = Anchor.CentreLeft,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (starRatingDisplay != null)
            {
                starDifficultyBindable = difficultyCache.GetBindableDifficulty(setInfo.Beatmaps.Single());

                starDifficultyBindable.BindValueChanged(d =>
                {
                    starRatingDisplay.Current.Value = d.NewValue ?? default;
                }, true);
            }
        }
    }
}
