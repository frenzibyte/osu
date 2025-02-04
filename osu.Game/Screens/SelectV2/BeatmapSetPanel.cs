// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
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
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        private const float arrow_container_width = 20;
        private const float difficulty_icon_container_width = 30;
        private const float corner_radius = 10;

        private const float duration = 500;

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

        private CancellationTokenSource? singleDiffStarsToken;
        private IBindable<StarDifficulty?>? singleDiffStarsBindable;

        private Container panel = null!;
        private Box backgroundBorder = null!;
        private BeatmapSetPanelBackground background = null!;
        private Container backgroundContainer = null!;
        private FillFlowContainer mainFlowContainer = null!;
        private Container selectionIconContainer = null!;
        private SpriteIcon chevronIcon = null!;
        private Box hoverLayer = null!;

        private readonly BindableBool expanded = new BindableBool();

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private UpdateBeatmapSetButtonV2 updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;
        private DifficultySpectrumDisplay difficultiesDisplay = null!;

        private ConstrainedIconContainer singleDiffIcon = null!;
        private FillFlowContainer singleDiffLine = null!;
        private StarRatingDisplay singleDiffStarRating = null!;
        private TopLocalRankV2 singleDiffRank = null!;
        private OsuSpriteText singleDiffName = null!;
        private OsuSpriteText singleDiffAuthor = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.X;
            Width = 1f;
            Height = HEIGHT;

            InternalChild = panel = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 10,
                },
                Children = new Drawable[]
                {
                    new BufferedContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            backgroundBorder = new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Alpha = 0,
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            backgroundContainer = new Container
                            {
                                Masking = true,
                                CornerRadius = corner_radius,
                                RelativeSizeAxes = Axes.X,
                                MaskingSmoothness = 2,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    background = new BeatmapSetPanelBackground
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        // Scale up a bit to cover the sheared edges.
                                        Scale = new Vector2(1.05f),
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientHorizontal(colourProvider.Background5.Opacity(0.5f), colourProvider.Background5.Opacity(0f)),
                                    }
                                },
                            },
                        }
                    },
                    selectionIconContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Alpha = 0,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.CentreLeft,
                        Children = new Drawable[]
                        {
                            singleDiffIcon = new ConstrainedIconContainer
                            {
                                X = difficulty_icon_container_width / 2,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                Size = new Vector2(20),
                            },
                            chevronIcon = new SpriteIcon
                            {
                                X = arrow_container_width / 2,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                Icon = FontAwesome.Solid.ChevronRight,
                                Size = new Vector2(12),
                                Colour = colourProvider.Background5,
                            },
                        }
                    },
                    mainFlowContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Top = 7.5f, Left = 15, Bottom = 5 },
                        Children = new Drawable[]
                        {
                            titleText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                                Shadow = true,
                            },
                            artistText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                                Shadow = true,
                            },
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 5f },
                                Children = new Drawable[]
                                {
                                    updateButton = new UpdateBeatmapSetButtonV2
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Right = 5f, Top = -2f },
                                    },
                                    statusPill = new BeatmapSetOnlineStatusPill
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        TextSize = 11,
                                        TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                        Margin = new MarginPadding { Right = 5f },
                                    },
                                    difficultiesDisplay = new DifficultySpectrumDisplay
                                    {
                                        DotSize = new Vector2(5, 10),
                                        DotSpacing = 2,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Alpha = 0f,
                                    },
                                    singleDiffLine = new FillFlowContainer
                                    {
                                        Direction = FillDirection.Horizontal,
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            singleDiffStarRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Margin = new MarginPadding { Right = 5f },
                                            },
                                            singleDiffRank = new TopLocalRankV2
                                            {
                                                Scale = new Vector2(8f / 11),
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Margin = new MarginPadding { Right = 5f },
                                            },
                                            singleDiffName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                                Origin = Anchor.BottomLeft,
                                                Anchor = Anchor.BottomLeft,
                                                Margin = new MarginPadding { Right = 5f, Bottom = 2f },
                                            },
                                            singleDiffAuthor = new OsuSpriteText
                                            {
                                                Colour = colourProvider.Content2,
                                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                                Origin = Anchor.BottomLeft,
                                                Anchor = Anchor.BottomLeft,
                                                Margin = new MarginPadding { Right = 5f, Bottom = 2f },
                                            }
                                        }
                                    },
                                },
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
                    new HoverSounds(),
                }
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = panel.DrawRectangle;

            // Cover a gap introduced by the spacing between a BeatmapSetPanel and a BeatmapPanel either above it or below it.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(panel.ToLocalSpace(screenSpacePos));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            expanded.BindValueChanged(_ => updateExpandedDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateKeyboardSelectedDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            Debug.Assert(Item.IsGroupSelectionTarget);

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            background.Beatmap = beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID));

            titleText.Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title);
            artistText.Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist);
            updateButton.BeatmapSet = beatmapSet;
            statusPill.Status = beatmapSet.Status;

            singleDiffStarsToken?.Cancel();
            singleDiffStarsToken = new CancellationTokenSource();
            singleDiffStarsBindable = null;

            if (beatmapSet.Beatmaps.Count == 1)
            {
                chevronIcon.Hide();
                difficultiesDisplay.Hide();

                var singleBeatmap = beatmapSet.Beatmaps.Single();

                singleDiffIcon.Icon = singleBeatmap.Ruleset.CreateInstance().CreateIcon();
                singleDiffIcon.Show();

                singleDiffRank.Beatmap = singleBeatmap;
                singleDiffName.Text = singleBeatmap.DifficultyName;
                singleDiffAuthor.Text = BeatmapsetsStrings.ShowDetailsMappedBy(singleBeatmap.Metadata.Author.Username);
                singleDiffLine.Show();

                computeSingleDiffStars(singleBeatmap, singleDiffStarsToken.Token);
            }
            else
            {
                singleDiffIcon.Hide();
                singleDiffLine.Hide();

                chevronIcon.Show();

                difficultiesDisplay.BeatmapSet = beatmapSet;
                difficultiesDisplay.Show();

                backgroundBorder.Colour = Color4.White;
                selectionIconContainer.Colour = Color4.White;
            }

            updateExpandedState();
            updateExpandedDisplay();
            FinishTransforms(true);

            this.FadeInFromZero(duration, Easing.OutQuint);
        }

        private void updateExpandedDisplay()
        {
            if (Item == null)
                return;

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            float selectionIndicatorWidth = beatmapSet.Beatmaps.Count == 1 ? difficulty_icon_container_width : arrow_container_width;

            updatePanelPosition();

            backgroundBorder.RelativeSizeAxes = expanded.Value ? Axes.Both : Axes.Y;
            backgroundBorder.Width = expanded.Value ? 1 : selectionIndicatorWidth + corner_radius;
            backgroundBorder.FadeTo(expanded.Value ? 1 : 0, duration, Easing.OutQuint);
            selectionIconContainer.FadeTo(expanded.Value ? 1 : 0, duration, Easing.OutQuint);

            backgroundContainer.ResizeHeightTo(expanded.Value ? HEIGHT - 4 : HEIGHT, duration, Easing.OutQuint);
            backgroundContainer.MoveToX(expanded.Value ? selectionIndicatorWidth : 0, duration, Easing.OutQuint);
            mainFlowContainer.MoveToX(expanded.Value ? selectionIndicatorWidth : 0, duration, Easing.OutQuint);

            panel.EdgeEffect = panel.EdgeEffect with { Radius = expanded.Value ? 15 : 10 };
            updateEdgeEffectColour();
        }

        private void updateKeyboardSelectedDisplay()
        {
            updatePanelPosition();
            updateHover();
        }

        private void updatePanelPosition()
        {
            if (expanded.Value)
                this.ResizeWidthTo(1f, duration, Easing.OutQuint);
            else if (KeyboardSelected.Value)
                this.ResizeWidthTo(0.95f, duration, Easing.OutQuint);
            else
                this.ResizeWidthTo(0.9f, duration, Easing.OutQuint);
        }

        private void updateHover()
        {
            bool hovered = IsHovered || KeyboardSelected.Value;

            if (hovered)
                hoverLayer.FadeIn(100, Easing.OutQuint);
            else
                hoverLayer.FadeOut(1000, Easing.OutQuint);
        }

        private void computeSingleDiffStars(BeatmapInfo beatmap, CancellationToken cancellationToken)
        {
            singleDiffStarsBindable = difficultyCache.GetBindableDifficulty(beatmap, cancellationToken);
            singleDiffStarsBindable.BindValueChanged(d =>
            {
                if (d.NewValue == null)
                    return;

                backgroundBorder.FadeColour(colours.ForStarDifficulty(d.NewValue.Value.Stars), duration, Easing.OutQuint);
                selectionIconContainer.FadeColour(d.NewValue.Value.Stars > 6.5f ? Colour4.White : colourProvider.Background5, duration, Easing.OutQuint);
                singleDiffStarRating.Current.Value = d.NewValue.Value;

                updateEdgeEffectColour();
            }, true);
        }

        private void updateEdgeEffectColour()
        {
            if (Item == null)
                return;

            Color4 colour;

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            if (expanded.Value)
            {
                if (beatmapSet.Beatmaps.Count == 1)
                {
                    Debug.Assert(singleDiffStarsBindable != null);
                    colour = colours.ForStarDifficulty(singleDiffStarsBindable.Value?.Stars ?? 0f).Opacity(0.5f);
                }
                else
                    colour = Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f);
            }
            else
                colour = Color4.Black.Opacity(0.4f);

            panel.FadeEdgeEffectTo(colour, duration, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateHover();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateHover();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel.CurrentSelection = Item!.Model;
            return true;
        }

        protected override void Update()
        {
            base.Update();
            updateExpandedState();
        }

        private void updateExpandedState()
        {
            // todo: this should be sourced from CarouselItem instead of arbitrarily comparing against current selection.
            var ourBeatmapSet = (BeatmapSetInfo?)Item?.Model;
            var expandedBeatmapSet = ((BeatmapInfo?)carousel.CurrentSelection)?.BeatmapSet;

            expanded.Value = ourBeatmapSet != null && expandedBeatmapSet?.Equals(ourBeatmapSet) == true;
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        BindableBool ICarouselPanel.Selected { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated()
        {
            // sets should never be activated.
            throw new InvalidOperationException();
        }

        #endregion
    }
}
