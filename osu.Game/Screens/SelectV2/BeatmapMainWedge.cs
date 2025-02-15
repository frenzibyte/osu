// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedge : CompositeDrawable
    {
        private const float transition_duration = 250;
        private const float corner_radius = 10;
        private const float border_weight = 2;

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

        protected BeatmapMainWedgeContent? Content { get; private set; }

        private BeatmapSetOnlineStatusPill statusPill = null!;
        private Container content = null!;
        private Box difficultyBorder = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public BeatmapMainWedge()
        {
            Width = 760f;
            Height = 210;
            Y = 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Shear = shear;
            Masking = true;
            Margin = new MarginPadding { Left = -corner_radius - 8 };
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
                    Padding = new MarginPadding { Right = border_weight, Vertical = border_weight },
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
            mods.BindValueChanged(_ => updateDisplay());
            updateDisplay();

            FinishTransforms(true);

            this.MoveToX(-150)
                .MoveToX(0, SongSelectV2.ENTER_DURATION, Easing.OutQuint)
                .FadeInFromZero(SongSelectV2.ENTER_DURATION / 3, Easing.In);
        }

        private Container? loadingInfo;

        private void updateDisplay()
        {
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
                        CornerRadius = corner_radius - border_weight,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new BeatmapMainWedgeBackground(beatmap.Value) { Shear = -Shear },
                            Content = new BeatmapMainWedgeContent(beatmap.Value) { Shear = -Shear }
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
    }
}
