// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    public class BeatmapInfoFooter : CompositeDrawable
    {
        private OsuTextFlowContainer totalPlayCount;
        private OsuTextFlowContainer difficultyPlayCount;

        [Resolved]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [Resolved]
        private IBindable<APIBeatmap> beatmap { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 7f,
                        Colour = colourProvider.Background4,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background3,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Horizontal = 50f, Vertical = 10f },
                                Children = new[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(20f, 0f),
                                        Margin = new MarginPadding { Left = 3f },
                                        Children = new Drawable[]
                                        {
                                            totalPlayCount = new OsuTextFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                            }.With(t =>
                                            {
                                            }),
                                            difficultyPlayCount = new OsuTextFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                            }.With(t =>
                                            {
                                            })
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        RelativeSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5f, 0f),
                                        Children = new Drawable[]
                                        {
                                            new BeatmapDownloadButton
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Current = { BindTarget = beatmapSet },
                                            },
                                            new BeatmapFavouriteButton
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                Current = { BindTarget = beatmapSet },
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSet.BindValueChanged(set =>
            {
                totalPlayCount.Clear();

                totalPlayCount.AddText("Total Play Count\n", s => s.Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold));
                totalPlayCount.AddText(set.NewValue?.Beatmaps.Select(b => b.PlayCount).Sum().ToString("N0") ?? "-", s =>
                {
                    s.Colour = colourProvider.Content2;
                    s.Font = OsuFont.Default.With(size: 14, weight: FontWeight.Regular);
                });
            }, true);

            beatmap.BindValueChanged(b =>
            {
                difficultyPlayCount.Clear();

                difficultyPlayCount.AddText("Difficulty Play Count\n", s => s.Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold));
                difficultyPlayCount.AddText(b.NewValue?.PlayCount.ToString("N0") ?? "-", s =>
                {
                    s.Colour = colourProvider.Content2;
                    s.Font = OsuFont.Default.With(size: 14, weight: FontWeight.Regular);
                });
            }, true);
        }
    }
}
