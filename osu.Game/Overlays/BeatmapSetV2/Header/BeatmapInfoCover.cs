// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence. // See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapInfoCover : CompositeDrawable
    {
        private UpdateableOnlineBeatmapSetCover cover;
        private IconButton expandButton;
        private IconButton previewButton;

        private readonly BindableBool coverExpanded = new BindableBool();
        private readonly BindableBool previewPlaying = new BindableBool();

        private BeatmapSetOnlineStatusPill statusPill;
        private BeatmapSetOnlineExplicitPill explicitPill;

        [Resolved]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                cover = new UpdateableOnlineBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.X,
                    Masking = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 50, Right = 10, Vertical = 12 },
                    Children = new[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(5f),
                            Children = new Drawable[]
                            {
                                statusPill = new BeatmapSetOnlineStatusPill
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Size = new Vector2(100f, 30f),
                                    BackgroundAlpha = 0.8f,
                                    TextSize = 14f,
                                },
                                explicitPill = new BeatmapSetOnlineExplicitPill
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Size = new Vector2(100f, 30f),
                                    BackgroundAlpha = 0.8f,
                                    TextSize = 14f,
                                }
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(10f),
                            Children = new[]
                            {
                                expandButton = new IconButton
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Size = new Vector2(30f),
                                    Action = coverExpanded.Toggle,
                                    Icon = { Icon = FontAwesome.Solid.ChevronDown },
                                },
                                previewButton = new IconButton
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Size = new Vector2(100f, 30f),
                                    Action = previewPlaying.Toggle,
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            coverExpanded.BindValueChanged(expanded =>
            {
                cover.ResizeHeightTo(expanded.NewValue ? 250f : 53f, 300.0, Easing.OutQuint);
                expandButton.Icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
            }, true);

            previewPlaying.BindValueChanged(playing =>
            {
                previewButton.Icon.Icon = playing.NewValue ? FontAwesome.Solid.Pause : FontAwesome.Solid.Play;
            }, true);

            beatmapSet.BindValueChanged(set =>
            {
                cover.OnlineInfo = set.NewValue;

                statusPill.Status = set.NewValue?.Status ?? BeatmapSetOnlineStatus.None;
                explicitPill.Alpha = set.NewValue?.HasExplicitContent == true ? 1f : 0f;
            }, true);

            FinishTransforms(true);
        }

        public class IconButton : OsuHoverContainer
        {
            private readonly Box background;
            public readonly SpriteIcon Icon;

            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public IconButton()
            {
                Child = new CircularContainer
                {
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            Alpha = 0.8f,
                            RelativeSizeAxes = Axes.Both,
                        },
                        Icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(12f),
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Icon.Colour = colourProvider.Content1;

                IdleColour = colourProvider.Background5;
                HoverColour = colourProvider.Background5.Lighten(0.25f);
            }
        }
    }
}
