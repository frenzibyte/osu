// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.SelectV2.Wedge;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedge : CompositeDrawable
    {
        private DifficultyNameContent difficultyLabel = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private ShearedBox difficultyBorder = null!;
        private ShearedBox difficultyTint = null!;

        private CancellationTokenSource? cancellationToken;
        private CancellationTokenSource? starsCancellationToken;

        private ContentWedgeItem densityBar = null!;
        private ContentWedgeItem failRateBar = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public IBindable<double> DisplayedStars => starRatingDisplay.DisplayedStars;

        public BeatmapContentWedge()
        {
            Width = 640f;
            Height = 100f;
            Y = 215f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const float content_margin = SongSelectV2.WEDGE_CONTENT_MARGIN;

            InternalChildren = new Drawable[]
            {
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10f,
                    Children = new[]
                    {
                        difficultyBorder = new ShearedBox
                        {
                            RelativeSizeAxes = Axes.Both,
                            DropShadow = false,
                            LeftPadded = true,
                        },
                        new ShearedBox
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.985f,
                            // slight adjustments to avoid bleeding.
                            Height = 1.02f,
                            Y = -1.5f,
                            LeftPadded = true,
                            Colour = colourProvider.Background5,
                        },
                        difficultyTint = new ShearedBox
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.5f,
                            X = -10f,
                            // slight adjustments to avoid bleeding.
                            Height = 1.02f,
                            Y = -1.5f,
                            Colour = Color4.Black.Opacity(0f),
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = content_margin, Vertical = 10f },
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 10f },
                            Children = new Drawable[]
                            {
                                difficultyLabel = new DifficultyNameContent(),
                                starRatingDisplay = new StarRatingDisplay(default)
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = -20f },
                                },
                            }
                        },
                        densityBar = new ContentWedgeItem(4)
                        {
                            Label = "Density",
                            RelativeSizeAxes = Axes.X,
                            Height = 10f,
                            TierColours = new List<Colour4>
                            {
                                Color4Extensions.FromHex("66CCFF"),
                                Color4Extensions.FromHex("44AADD"),
                                Color4Extensions.FromHex("1188AA"),
                                Color4Extensions.FromHex("303D47"),
                            },
                        },
                        failRateBar = new ContentWedgeItem(6)
                        {
                            Label = "Fail Rate",
                            RelativeSizeAxes = Axes.X,
                            Height = 10f,
                            TierColours = new List<Colour4>
                            {
                                Color4Extensions.FromHex("AADD00"),
                                Color4Extensions.FromHex("FFDD55"),
                                Color4Extensions.FromHex("FFCC22"),
                                Color4Extensions.FromHex("EEAA00"),
                                Color4Extensions.FromHex("EB5757"),
                                Color4Extensions.FromHex("643232"),
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            starRatingDisplay.DisplayedStars.BindValueChanged(v =>
            {
                Color4 colour = colours.ForStarDifficulty(v.NewValue);
                difficultyBorder.Colour = colour;
                difficultyTint.Colour = ColourInfo.GradientHorizontal(colour.Opacity(0f), colour.Opacity(0.2f));
            }, true);

            beatmap.BindValueChanged(_ => updateBeatmap());
            ruleset.BindValueChanged(_ => updateStarRatingDisplay());
            mods.BindValueChanged(_ => updateStarRatingDisplay());
            updateBeatmap();
        }

        private void updateBeatmap()
        {
            cancellationToken?.Cancel();
            cancellationToken = new CancellationTokenSource();

            if (beatmap.Value is null or DummyWorkingBeatmap)
            {
                this.MoveToX(-150, SongSelectV2.ENTER_DURATION, Easing.OutQuint)
                    .FadeOut(SongSelectV2.ENTER_DURATION / 3, Easing.OutSine);

                return;
            }

            this.MoveToX(0, SongSelectV2.ENTER_DURATION, Easing.OutQuint)
                .FadeIn(SongSelectV2.ENTER_DURATION / 3, Easing.In);

            difficultyLabel.Value = new DifficultyNameContent.Data(beatmap.Value.BeatmapInfo.DifficultyName, beatmap.Value.Metadata.Author);
            densityBar.Values = BeatmapExtensions.CalculateDensity(beatmap.Value.Beatmap.HitObjects, 100);

            failRateBar.Values = null;

            var token = cancellationToken.Token;

            beatmapLookupCache.GetBeatmapAsync(beatmap.Value.BeatmapInfo.OnlineID, token)
                              .ContinueWith(task =>
                              {
                                  Schedule(() =>
                                  {
                                      if (token.IsCancellationRequested)
                                          return;

                                      APIBeatmap? result = task.GetResultSafely();
                                      failRateBar.Values = result?.FailTimes?.Fails ?? new int[100];
                                  });
                              }, token);
        }

        private void updateStarRatingDisplay()
        {
            starsCancellationToken?.Cancel();
            starsCancellationToken = new CancellationTokenSource();

            if (beatmap.Value is null or DummyWorkingBeatmap)
                return;

            var token = starsCancellationToken.Token;

            difficultyCache.GetDifficultyAsync(beatmap.Value.BeatmapInfo, ruleset.Value, mods.Value, token)
                           .ContinueWith(task =>
                           {
                               Schedule(() =>
                               {
                                   if (token.IsCancellationRequested)
                                       return;

                                   StarDifficulty? result = task.GetResultSafely();
                                   starRatingDisplay.Current.Value = result ?? default;
                               });
                           }, token);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            cancellationToken?.Cancel();
            starsCancellationToken?.Cancel();
        }

        private partial class ContentWedgeItem : CompositeDrawable
        {
            private readonly OsuSpriteText labelText;
            private readonly SegmentedGraph<int> graph;
            private readonly Box placeholder;

            public required string Label
            {
                init => labelText.Text = value;
            }

            public required IReadOnlyList<Colour4> TierColours
            {
                init => graph.TierColours = value;
            }

            private int[]? values;

            public int[]? Values
            {
                set
                {
                    if (values == value)
                        return;

                    values = value;
                    updateValues();
                }
            }

            public ContentWedgeItem(int tiersCount)
            {
                InternalChildren = new Drawable[]
                {
                    labelText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                        Y = -1f,
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.85f,
                        Masking = true,
                        // todo: this destroys the smoothness of the circular edges but it's required to avoid one pixel line artifacts.
                        MaskingSmoothness = 0,
                        Children = new Drawable[]
                        {
                            graph = new SegmentedGraph<int>(tiersCount)
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            placeholder = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0f,
                                Colour = Color4.Black,
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateValues();
            }

            private void updateValues()
            {
                ClearTransforms(true);

                if (values == null)
                {
                    placeholder.FadeTo(0.2f, 500)
                               .Then()
                               .FadeTo(0.5f, 500)
                               .Loop();
                }
                else
                    placeholder.FadeOut(500);

                graph.Values = values ?? Array.Empty<int>();
            }
        }
    }
}
