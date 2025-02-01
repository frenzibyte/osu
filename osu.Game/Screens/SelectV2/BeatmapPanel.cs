// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Carousel;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private const float colour_box_width = 30;
        private const float corner_radius = 10;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private Container header = null!;
        private StarCounter starCounter = null!;
        private ConstrainedIconContainer iconContainer = null!;
        private Box hoverLayer = null!;

        private Box colourBox = null!;

        private StarRatingDisplay starRatingDisplay = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private OsuSpriteText keyCountText = null!;

        private IBindable<StarDifficulty?> starDifficultyBindable = null!;
        private CancellationTokenSource? starDifficultyCancellationSource;
        private Container rightContainer = null!;
        private Box starRatingGradient = null!;
        private Container topLocalRankContainer = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText authorText = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            Width = 1.25f;
            Height = HEIGHT;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    header = new Container
                    {
                        Name = "CarouselHeader",
                        Shear = shear,
                        Masking = true,
                        CornerRadius = corner_radius,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    colourBox = new Box
                                    {
                                        Width = colour_box_width + corner_radius,
                                        RelativeSizeAxes = Axes.Y,
                                        Colour = colours.ForStarDifficulty(0),
                                        EdgeSmoothness = new Vector2(2, 0),
                                    },
                                    rightContainer = new Container
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Masking = true,
                                        CornerRadius = corner_radius,
                                        RelativeSizeAxes = Axes.X,
                                        // We don't want to match the header's size when its selected, hence no relative sizing.
                                        Height = HEIGHT,
                                        X = colour_box_width,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4),
                                            },
                                            starRatingGradient = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Alpha = 0,
                                            },
                                        },
                                    },
                                }
                            },
                            iconContainer = new ConstrainedIconContainer
                            {
                                X = colour_box_width / 2,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.CentreLeft,
                                Size = new Vector2(20),
                                Colour = colourProvider.Background5,
                                Shear = -shear,
                            },
                            new FillFlowContainer
                            {
                                Padding = new MarginPadding { Top = 8, Left = colour_box_width + corner_radius },
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(3, 0),
                                        AutoSizeAxes = Axes.Both,
                                        Shear = -shear,
                                        Children = new Drawable[]
                                        {
                                            starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small),
                                            topLocalRankContainer = new Container
                                            {
                                                AutoSizeAxes = Axes.Both,
                                            },
                                            starCounter = new StarCounter
                                            {
                                                Margin = new MarginPadding { Top = 8 }, // Better aligns the stars with the star rating display
                                                Scale = new Vector2(8 / 20f)
                                            }
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(11, 0),
                                        AutoSizeAxes = Axes.Both,
                                        Shear = -shear,
                                        Children = new[]
                                        {
                                            keyCountText = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Alpha = 0,
                                            },
                                            difficultyText = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft
                                            },
                                            authorText = new OsuSpriteText
                                            {
                                                Colour = colourProvider.Content2,
                                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft
                                            }
                                        }
                                    }
                                }
                            },
                            hoverLayer = new Box
                            {
                                Colour = colours.Blue.Opacity(0.1f),
                                Alpha = 0,
                                Blending = BlendingParameters.Additive,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                    new BeatmapSetPanel.HeaderSounds(),
                }
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = DrawRectangle;

            // Cover the gaps introduced by the spacing between BeatmapPanels.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(ToLocalSpace(screenSpacePos));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Selected.BindValueChanged(_ => updateSelectionDisplay());
            KeyboardSelected.BindValueChanged(_ => updateSelectionDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            var beatmap = (BeatmapInfo)Item.Model;

            iconContainer.Icon = beatmap.Ruleset.CreateInstance().CreateIcon();

            //Scaling is applied to size match components of row
            topLocalRankContainer.Child = new TopLocalRank(beatmap) { Scale = new Vector2(8f / 11) };

            difficultyText.Text = beatmap.DifficultyName;
            authorText.Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmap.Metadata.Author.Username);

            // todo: replay star counter animation when a carousel set is expanded.
            // if (Item?.State.Value != CarouselItemState.Collapsed && Alpha == 0)
            //     starCounter.ReplayAnimation();

            starDifficultyCancellationSource?.Cancel();

            // Only compute difficulty when the item is visible.
            // if (Item?.State.Value != CarouselItemState.Collapsed)
            // {
            // We've potentially cancelled the computation above so a new bindable is required.
            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, (starDifficultyCancellationSource = new CancellationTokenSource()).Token);
            starDifficultyBindable.BindValueChanged(d =>
            {
                starRatingDisplay.Current.Value = d.NewValue ?? default;
            }, true);

            starRatingDisplay.Current.BindValueChanged(s =>
            {
                starCounter.Current = (float)s.NewValue.Stars;

                // Every other element in song select that uses this cut off uses yellow for the upper range but the designs use white here for whatever reason.
                iconContainer.Colour = s.NewValue.Stars > 6.5f ? Colour4.White : colourProvider.Background5;

                var starRatingColour = colours.ForStarDifficulty(s.NewValue.Stars);

                starCounter.Colour = colourBox.Colour = starRatingColour;
                starRatingGradient.Colour = ColourInfo.GradientHorizontal(starRatingColour.Opacity(0.25f), starRatingColour.Opacity(0));
                starRatingGradient.Show();
            });

            updateKeyCount();
            // }
            this.FadeInFromZero(500, Easing.OutQuint);
            updateSelectionDisplay();
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverLayer.FadeIn(100, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(1000, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private void updateSelectionDisplay()
        {
            bool selected = Selected.Value;

            // todo: completely missing keyboard selection feedback.
            if (selected)
            {
                header.MoveToX(25f, 500, Easing.OutExpo);

                rightContainer.Height = HEIGHT - 4;

                colourBox.RelativeSizeAxes = Axes.Both;
                colourBox.Width = 1;
                header.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = starCounter.Colour.MultiplyAlpha(0.5f),
                    Radius = 10,
                };
            }
            else
            {
                header.MoveToX(75f, 500, Easing.OutExpo);

                rightContainer.Height = HEIGHT;

                header.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Offset = new Vector2(1),
                    Radius = 10,
                    Colour = Colour4.Black.Opacity(100),
                };

                colourBox.RelativeSizeAxes = Axes.Y;
                colourBox.Width = colour_box_width + corner_radius;
            }
        }

        private void updateKeyCount()
        {
            // if (Item?.State.Value == CarouselItemState.Collapsed)
            //     return;

            var beatmap = (BeatmapInfo)Item!.Model;

            if (ruleset.Value.OnlineID == 3)
            {
                // Account for mania differences locally for now.
                // Eventually this should be handled in a more modular way, allowing rulesets to add more information to the panel.
                ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();

                keyCountText.Alpha = 1;
                keyCountText.Text = $"[{legacyRuleset.GetKeyCount(beatmap, mods.Value)}K]";
            }
            else
                keyCountText.Alpha = 0;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel.CurrentSelection != Item!.Model)
            {
                carousel.CurrentSelection = Item!.Model;
                return true;
            }

            carousel.TryActivateSelection();
            return true;
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated()
        {
            // activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }

        #endregion
    }
}
