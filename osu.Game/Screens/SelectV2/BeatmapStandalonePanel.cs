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
    public partial class BeatmapStandalonePanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        private const float difficulty_icon_container_width = 30;
        private const float corner_radius = 10;

        private const float preselected_x_offset = -25f;
        private const float selected_x_offset = -50f;

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

        private CancellationTokenSource? starDifficultyToken;
        private IBindable<StarDifficulty?>? starDifficultyBindable;

        private Container panel = null!;
        private Box backgroundBorder = null!;
        private BeatmapSetPanelBackground background = null!;
        private Container backgroundContainer = null!;
        private FillFlowContainer mainFlowContainer = null!;
        private Box hoverLayer = null!;

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private UpdateBeatmapSetButtonV2 updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;

        private ConstrainedIconContainer difficultyIcon = null!;
        private FillFlowContainer difficultyLine = null!;
        private StarRatingDisplay difficultyStarRating = null!;
        private TopLocalRankV2 difficultyRank = null!;
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText difficultyAuthor = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.X;
            Width = 1f;
            Height = HEIGHT;

            Padding = new MarginPadding { Right = preselected_x_offset + selected_x_offset };

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
                    difficultyIcon = new ConstrainedIconContainer
                    {
                        X = difficulty_icon_container_width / 2,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.CentreLeft,
                        Size = new Vector2(20),
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
                                    difficultyLine = new FillFlowContainer
                                    {
                                        Direction = FillDirection.Horizontal,
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            difficultyStarRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                                            {
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Margin = new MarginPadding { Right = 5f },
                                            },
                                            difficultyRank = new TopLocalRankV2
                                            {
                                                Scale = new Vector2(8f / 11),
                                                Origin = Anchor.CentreLeft,
                                                Anchor = Anchor.CentreLeft,
                                                Margin = new MarginPadding { Right = 5f },
                                            },
                                            difficultyName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                                Origin = Anchor.BottomLeft,
                                                Anchor = Anchor.BottomLeft,
                                                Margin = new MarginPadding { Right = 5f, Bottom = 2f },
                                            },
                                            difficultyAuthor = new OsuSpriteText
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

            Selected.BindValueChanged(_ => updateSelectedDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateKeyboardSelectedDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            Debug.Assert(Item.IsGroupSelectionTarget);

            var beatmapSet = (BeatmapSetInfo)Item.Model;
            var singleBeatmap = beatmapSet.Beatmaps.Single();

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            background.Beatmap = beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID));

            titleText.Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title);
            artistText.Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist);
            updateButton.BeatmapSet = beatmapSet;
            statusPill.Status = beatmapSet.Status;

            starDifficultyToken?.Cancel();
            starDifficultyToken = new CancellationTokenSource();
            starDifficultyBindable = null;

            difficultyIcon.Icon = singleBeatmap.Ruleset.CreateInstance().CreateIcon();
            difficultyIcon.Show();

            difficultyRank.Beatmap = singleBeatmap;
            difficultyName.Text = singleBeatmap.DifficultyName;
            difficultyAuthor.Text = BeatmapsetsStrings.ShowDetailsMappedBy(singleBeatmap.Metadata.Author.Username);
            difficultyLine.Show();

            computeStarRating(singleBeatmap, starDifficultyToken.Token);

            updateSelectedDisplay();
            FinishTransforms(true);

            this.FadeInFromZero(duration, Easing.OutQuint);
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            background.Beatmap = null;
            updateButton.BeatmapSet = null;
            difficultyRank.Beatmap = null;
        }

        private void updateSelectedDisplay()
        {
            if (Item == null)
                return;

            updatePanelPosition();

            backgroundBorder.RelativeSizeAxes = Selected.Value ? Axes.Both : Axes.Y;
            backgroundBorder.Width = Selected.Value ? 1 : difficulty_icon_container_width + corner_radius;
            backgroundBorder.FadeTo(Selected.Value ? 1 : 0, duration, Easing.OutQuint);
            difficultyIcon.FadeTo(Selected.Value ? 1 : 0, duration, Easing.OutQuint);

            backgroundContainer.ResizeHeightTo(Selected.Value ? HEIGHT - 4 : HEIGHT, duration, Easing.OutQuint);
            backgroundContainer.MoveToX(Selected.Value ? difficulty_icon_container_width : 0, duration, Easing.OutQuint);
            mainFlowContainer.MoveToX(Selected.Value ? difficulty_icon_container_width : 0, duration, Easing.OutQuint);

            panel.EdgeEffect = panel.EdgeEffect with { Radius = Selected.Value ? 15 : 10 };
            updateEdgeEffectColour();
        }

        private void updateKeyboardSelectedDisplay()
        {
            updatePanelPosition();
            updateHover();
        }

        private void updatePanelPosition()
        {
            float x = 0f;

            if (Selected.Value)
                x += selected_x_offset;

            if (KeyboardSelected.Value)
                x += preselected_x_offset;

            panel.MoveToX(x, duration, Easing.OutQuint);
        }

        private void updateHover()
        {
            bool hovered = IsHovered || KeyboardSelected.Value;

            if (hovered)
                hoverLayer.FadeIn(100, Easing.OutQuint);
            else
                hoverLayer.FadeOut(1000, Easing.OutQuint);
        }

        private void computeStarRating(BeatmapInfo beatmap, CancellationToken cancellationToken)
        {
            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, cancellationToken);
            starDifficultyBindable.BindValueChanged(d =>
            {
                if (d.NewValue == null)
                    return;

                backgroundBorder.FadeColour(colours.ForStarDifficulty(d.NewValue.Value.Stars), duration, Easing.OutQuint);
                difficultyIcon.FadeColour(d.NewValue.Value.Stars > 6.5f ? Colour4.White : colourProvider.Background5, duration, Easing.OutQuint);
                difficultyStarRating.Current.Value = d.NewValue.Value;

                updateEdgeEffectColour();
            }, true);
        }

        private void updateEdgeEffectColour()
        {
            panel.FadeEdgeEffectTo(Selected.Value
                ? colours.ForStarDifficulty(starDifficultyBindable?.Value?.Stars ?? 0f).Opacity(0.5f)
                : Color4.Black.Opacity(0.4f), duration, Easing.OutQuint);
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
    }
}
