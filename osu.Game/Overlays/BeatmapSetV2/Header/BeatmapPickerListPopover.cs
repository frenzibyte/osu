// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSetV2.Info;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapPickerListPopover : OsuPopover
    {
        public BeatmapPickerListPopover(IReadOnlyList<APIBeatmap> beatmaps)
            : base(false)
        {
            Body.CornerRadius = 5f;
            Body.Margin = new MarginPadding(5f);

            Child = new FillFlowContainer<BeatmapInfoTitleStrip>
            {
                AutoSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 20f, Vertical = 10f },
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5f),
                ChildrenEnumerable = beatmaps.Select(beatmap => new BeatmapInfoTitleStrip(true, false)
                {
                    AutoSizeAxes = Axes.Both,
                    Current = { Value = beatmap }
                }),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Background.Colour = colourProvider.Background3;
        }
    }
}
