// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    public class BeatmapInfoDetails : CompositeDrawable
    {
        [Resolved]
        private Bindable<APIBeatmap> beatmap { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background5,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = 50f, Vertical = 12.5f },
                    Children = new Drawable[]
                    {
                        new BeatmapInfoTitleStrip(true, true)
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Current = beatmap
                        },
                        new BeatmapDensityBar
                        {
                            Margin = new MarginPadding { Top = 10f },
                            RelativeSizeAxes = Axes.X,
                            Height = 30f,
                            Current = beatmap,
                        },
                        new BeatmapFailuresBar
                        {
                            Margin = new MarginPadding { Top = 5f },
                            RelativeSizeAxes = Axes.X,
                            Height = 10f,
                            Current = beatmap,
                        },
                        new BeatmapMetadataTable
                        {
                            Margin = new MarginPadding { Top = 15f },
                        }
                    }
                }
            };
        }
    }
}
