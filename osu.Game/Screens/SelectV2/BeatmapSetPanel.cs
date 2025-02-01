// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Carousel;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        private const float arrow_container_width = 20;
        private const float difficulty_icon_container_width = 30;
        private const float corner_radius = 10;

        private const float duration = 500;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private BeatmapCarousel carousel { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource? starDifficultyCancellationSource;
        private IBindable<StarDifficulty?>? starDifficultyBindable;

        private Container header = null!;
        private Box colourBox = null!;
        private Container backgroundContainer = null!;
        private Container backgroundContentContainer = null!;
        private Container mainFlowContainer = null!;
        private Box backgroundPlaceholder = null!;
        private Container iconContainer = null!;
        private Box hoverLayer = null!;

        private readonly BindableBool expanded = new BindableBool();

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Width = 1.25f;
            Height = HEIGHT;

            InternalChild = header = new Container
            {
                Name = "CarouselHeader",
                Shear = shear,
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
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
                                        RelativeSizeAxes = Axes.Y,
                                        Alpha = 0,
                                        EdgeSmoothness = new Vector2(2, 0),
                                    },
                                    backgroundContentContainer = new Container
                                    {
                                        Masking = true,
                                        CornerRadius = corner_radius,
                                        RelativeSizeAxes = Axes.X,
                                        MaskingSmoothness = 2,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            backgroundPlaceholder = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background5,
                                                Alpha = 0,
                                            },
                                            backgroundContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                        },
                                    },
                                }
                            },
                            iconContainer = new Container
                            {
                                AutoSizeAxes = Axes.Both,
                                Shear = -shear,
                                Alpha = 0,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.CentreLeft,
                            },
                            mainFlowContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
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
                    new HeaderSounds(),
                }
            };

            // Header.AddRange(drawables);
            // drawables.ForEach(d => d.FadeInFromZero(150));
            // updateSelectionState();
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = DrawRectangle;

            // Cover a gap that could be introduced by the spacing between a BeatmapSetPanel and a BeatmapPanel either above it or below it.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(ToLocalSpace(screenSpacePos));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            expanded.BindValueChanged(_ => updateSelectionDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateSelectionDisplay());
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            Debug.Assert(Item.IsGroupSelectionTarget);

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            backgroundContainer.Child = new SetPanelBackground(beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID)))
            {
                RelativeSizeAxes = Axes.Both,
                Shear = -shear,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            };

            mainFlowContainer.Child = new SetPanelContentV2(beatmapSet)
            {
                RelativeSizeAxes = Axes.Both
            };

            if (beatmapSet.Beatmaps.Count == 1)
            {
                iconContainer.Add(new ConstrainedIconContainer
                {
                    X = difficulty_icon_container_width / 2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Icon = beatmapSet.Beatmaps.Single().Ruleset.CreateInstance().CreateIcon(),
                    Size = new Vector2(20),
                });
            }
            else
            {
                iconContainer.Add(new SpriteIcon
                {
                    X = arrow_container_width / 2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(12),
                    // TODO: implement colour sampling of beatmap background
                    Colour = colourProvider.Background5,
                });
            }

            this.FadeInFromZero(500, Easing.OutQuint);
            updateSelectionDisplay();
        }

        protected override void Update()
        {
            base.Update();

            var ourBeatmapSet = (BeatmapSetInfo?)Item?.Model;
            var expandedBeatmapSet = ((BeatmapInfo?)carousel.CurrentSelection)?.BeatmapSet;
            expanded.Value = ourBeatmapSet != null && expandedBeatmapSet?.Equals(ourBeatmapSet) == true;
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
            if (Item == null)
                return;

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            float colourBoxWidth = beatmapSet.Beatmaps.Count == 1 ? difficulty_icon_container_width : arrow_container_width;

            bool selected = expanded.Value;

            header.MoveToX(selected ? -75f : 0, duration, Easing.OutQuint);

            backgroundContentContainer.Height = selected ? HEIGHT - 4 : HEIGHT;

            // TODO: implement colour sampling of beatmap background for colour box and offset this by 10, hide for now
            backgroundContentContainer.MoveToX(selected ? colourBoxWidth : 0, duration, Easing.OutQuint);
            mainFlowContainer.MoveToX(selected ? colourBoxWidth : 0, duration, Easing.OutQuint);

            colourBox.RelativeSizeAxes = selected ? Axes.Both : Axes.Y;
            colourBox.Width = selected ? 1 : colourBoxWidth + corner_radius;
            colourBox.FadeTo(selected ? 1 : 0, duration, Easing.OutQuint);
            iconContainer.FadeTo(selected ? 1 : 0, duration, Easing.OutQuint);
            backgroundPlaceholder.FadeTo(selected ? 1 : 0, duration, Easing.OutQuint);

            starDifficultyCancellationSource?.Cancel();

            if (beatmapSet.Beatmaps.Count == 1)
            {
                starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmapSet.Beatmaps.Single(), (starDifficultyCancellationSource = new CancellationTokenSource()).Token);
                starDifficultyBindable.BindValueChanged(d =>
                {
                    // We want to update the EdgeEffect here instead of in value() to make sure the colours are correct
                    if (d.NewValue != null)
                    {
                        colourBox.Colour = colours.ForStarDifficulty(d.NewValue.Value.Stars);
                        iconContainer.Colour = d.NewValue.Value.Stars > 6.5f ? Colour4.White : colourProvider.Background5;

                        header.EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = selected ? colours.ForStarDifficulty(d.NewValue.Value.Stars).Opacity(0.5f) : Colour4.Black.Opacity(100),
                            Radius = selected ? 15 : 10,
                        };
                    }
                }, true);
            }
            else
            {
                header.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = selected ? Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f) : Colour4.Black.Opacity(100),
                    Radius = selected ? 15 : 10,
                };
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.CurrentSelection = Item!.Model;
            return true;
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated()
        {
            // sets should never be activated.
            throw new InvalidOperationException();
        }

        #endregion

        public partial class HeaderSounds : HoverSampleDebounceComponent
        {
            private Sample? sampleHover;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                sampleHover = audio.Samples.Get("UI/default-hover");
            }

            public override void PlayHoverSample()
            {
                if (sampleHover == null) return;

                sampleHover.Frequency.Value = 0.99 + RNG.NextDouble(0.02);
                sampleHover.Play();
            }
        }
    }
}
