// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapBackgroundWedge : VisibilityContainer
    {
        public const float WEDGE_HEIGHT = 210;
        private const float shear_width = OsuGame.SHEAR * WEDGE_HEIGHT;
        private const float transition_duration = 250;
        private const float corner_radius = 10;
        private const float colour_bar_width = 30;

        /// Todo: move this const out to song select when more new design elements are implemented for the beatmap details area, since it applies to text alignment of various elements
        private const float text_margin = 62;

        private const double animation_duration = 600;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        protected Container? DisplayedContent { get; private set; }

        protected WedgeInfoText? Info { get; private set; }

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container content = null!;
        private Box difficultyBorder = null!;

        private IBindable<StarDifficulty?>? starDifficulty;
        private CancellationTokenSource? cancellationSource;

        private WorkingBeatmap beatmap = null!;

        public WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value) return;

                beatmap = value;

                updateDisplay();
            }
        }

        public BeatmapBackgroundWedge()
        {
            Width = 700f;
            Y = 90;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = WEDGE_HEIGHT;
            Shear = shear;
            Masking = true;
            Margin = new MarginPadding { Left = -corner_radius };
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Colour4.Black.Opacity(0.2f),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
            };
            CornerRadius = corner_radius;

            Children = new Drawable[]
            {
                difficultyBorder = new Box
                {
                    Colour = Color4.Red,
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 3f, Vertical = 2.5f },
                    Children = new Drawable[]
                    {
                        statusPill = new BeatmapSetOnlineStatusPill
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Shear = -shear,
                            Margin = new MarginPadding { Right = 20f, Top = 10f },
                            TextSize = 11,
                            TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                            Alpha = 0,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateDisplay());
        }

        protected override void PopIn()
        {
            this.MoveToX(0, animation_duration, Easing.OutQuint);
            this.FadeIn(200, Easing.In);
        }

        protected override void PopOut()
        {
            this.MoveToX(-150, animation_duration, Easing.OutQuint);
            this.FadeOut(200, Easing.OutQuint);
        }

        private Container? loadingInfo;

        private void updateDisplay()
        {
            statusPill.Status = beatmap.BeatmapInfo.Status;

            starDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);

            starDifficulty.BindValueChanged(s =>
            {
                double stars = s.NewValue?.Stars ?? 0;
                difficultyBorder.FadeColour(colours.ForStarDifficulty(stars), 300, Easing.OutQuint);
            });

            Scheduler.AddOnce(() =>
            {
                LoadComponentAsync(loadingInfo = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = DisplayedContent?.Depth + 1 ?? 0,
                    Child = new Container
                    {
                        Masking = true,
                        CornerRadius = corner_radius,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            // TODO: New wedge design uses a coloured horizontal gradient for its background, however this lacks implementation information in the figma draft.
                            // pending https://www.figma.com/file/DXKwqZhD5yyb1igc3mKo1P?node-id=2980:3361#340801912 being answered.
                            new BeatmapInfoWedgeBackground(beatmap) { Shear = -Shear },
                            Info = new WedgeInfoText(beatmap) { Shear = -Shear }
                        }
                    }
                }, d =>
                {
                    // Ensure we are the most recent loaded wedge.
                    if (d != loadingInfo) return;

                    removeOldInfo();
                    content.Add(DisplayedContent = d);
                });
            });

            void removeOldInfo()
            {
                DisplayedContent?.FadeOut(transition_duration);
                DisplayedContent?.Expire();
                DisplayedContent = null;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationSource?.Cancel();
        }

        public partial class WedgeInfoText : Container
        {
            public OsuSpriteText TitleLabel { get; private set; } = null!;
            public OsuSpriteText ArtistLabel { get; private set; } = null!;

            private readonly WorkingBeatmap working;

            public WedgeInfoText(WorkingBeatmap working)
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

                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = text_margin + 8f, Top = 12 },
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
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
                                Font = OsuFont.TorusAlternate.With(size: 40, weight: FontWeight.SemiBold),
                            },
                        },
                        new OsuHoverContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Action = () => songSelect?.Search(artistText.GetPreferred(localisation.CurrentParameters.Value.PreferOriginalScript)),
                            Child = ArtistLabel = new TruncatingSpriteText
                            {
                                // TODO : figma design has a diffused shadow, instead of the solid one present here, not possible currently as far as i'm aware.
                                Shadow = true,
                                Text = artistText,
                                // Not sure if this should be semi bold or medium
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                            },
                        },
                    }
                };
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                // best effort to confine the auto-sized text to wedge bounds
                // the artist label doesn't have an extra text_margin as it doesn't touch the right metadata
                TitleLabel.MaxWidth = DrawWidth - text_margin * 2 - shear_width;
                ArtistLabel.MaxWidth = DrawWidth - text_margin - shear_width;
            }
        }
    }
}
