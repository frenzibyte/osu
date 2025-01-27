// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2.Wedge;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedge : VisibilityContainer
    {
        private const double animation_duration = 600;

        /// Todo: move this const out to song select when more new design elements are implemented for the beatmap details area, since it applies to text alignment of various elements
        private const float text_margin = 62;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private StarRatingDisplay starRatingDisplay = null!;
        private Container difficultyBorder = null!;
        private Container difficultyTint = null!;

        private IBindable<StarDifficulty?>? starDifficulty;
        private CancellationTokenSource? cancellationSource;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public BeatmapContentWedge()
        {
            Y = 215f;
            Width = 640f;
            Height = 100f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 10f;

            InternalChildren = new Drawable[]
            {
                difficultyBorder = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = shear,
                    CornerRadius = 10f,
                    Masking = true,
                    Margin = new MarginPadding { Left = -10f },
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.985f,
                    // slight adjustments to avoid bleeding.
                    Height = 1.02f,
                    Y = -1.5f,
                    Shear = shear,
                    CornerRadius = 10f,
                    Masking = true,
                    Colour = colourProvider.Background5,
                    Margin = new MarginPadding { Left = -10f },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Colour4.Black.Opacity(0.2f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
                    },
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                difficultyTint = new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0f),
                    Width = 0.5f,
                    // slight adjustments to avoid bleeding.
                    Height = 1.02f,
                    Y = -1.5f,
                    Shear = shear,
                    CornerRadius = 10f,
                    X = -10f,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = text_margin + 2f, Right = 40f, Vertical = 10f },
                    Children = new Drawable[]
                    {
                        new LocalDifficultyNameContent(),
                        starRatingDisplay = new StarRatingDisplay(default)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Alpha = 0f,
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            starDifficulty = difficultyCache.GetBindableDifficulty(beatmap.Value.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);
            starDifficulty.BindValueChanged(s =>
            {
                starRatingDisplay.Current.Value = s.NewValue ?? default;
                starRatingDisplay.Show();
            }, true);

            starRatingDisplay.DisplayedStars.BindValueChanged(v =>
            {
                Color4 colour = colours.ForStarDifficulty(v.NewValue);
                difficultyBorder.FadeColour(colour, 300, Easing.OutQuint);
                difficultyTint.FadeColour(ColourInfo.GradientHorizontal(colour.Opacity(0f), colour.Opacity(0.2f)), 300, Easing.OutQuint);
            }, true);

            FinishTransforms(true);
        }

        protected override void PopIn()
        {
            this.MoveToX(0, animation_duration, Easing.OutQuint);
            this.FadeIn(200, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.MoveToX(-150, animation_duration, Easing.OutQuint);
            this.FadeOut(200, Easing.OutQuint);
        }
    }
}
