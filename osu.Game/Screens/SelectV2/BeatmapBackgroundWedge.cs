// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapBackgroundWedge : CompositeDrawable
    {
        private const float transition_duration = 250;
        private const float corner_radius = 10;

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
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        protected Container? DisplayedContent { get; private set; }

        protected WedgeInfoText? Info { get; private set; }

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container content = null!;
        private Box difficultyBorder = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public BeatmapBackgroundWedge()
        {
            Width = 700f;
            Height = 210;
            Y = 90;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

            InternalChildren = new Drawable[]
            {
                difficultyBorder = new Box
                {
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

            displayedStars.BindValueChanged(s =>
            {
                difficultyBorder.Colour = colours.ForStarDifficulty(s.NewValue);
            }, true);

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            updateDisplay();

            FinishTransforms(true);

            this.MoveToX(-150)
                .MoveToX(0, SongSelectV2.ENTER_DURATION, Easing.OutQuint)
                .FadeInFromZero(SongSelectV2.ENTER_DURATION / 3, Easing.In);
        }

        private Container? loadingInfo;

        private void updateDisplay()
        {
            statusPill.Status = beatmap.Value.BeatmapInfo.Status;

            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            computeStarDifficulty(cancellationSource.Token);

            Schedule(() =>
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
                            new BeatmapInfoWedgeBackground(beatmap.Value) { Shear = -Shear },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(colourProvider.Background6.Opacity(0.75f), colourProvider.Background6),
                            },
                            Info = new WedgeInfoText(beatmap.Value) { Shear = -Shear }
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

            private const float content_margin = SongSelectV2.WEDGE_CONTENT_MARGIN;

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
                    Padding = new MarginPadding { Left = content_margin + 6f, Top = 12 },
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
                                Shadow = true,
                                Text = artistText,
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
                float shearWidth = OsuGame.SHEAR * DrawHeight;

                TitleLabel.MaxWidth = DrawWidth - content_margin * 2 - shearWidth;
                ArtistLabel.MaxWidth = DrawWidth - content_margin - shearWidth;
            }
        }
    }
}
