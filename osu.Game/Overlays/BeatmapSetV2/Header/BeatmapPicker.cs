// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapPicker : CompositeDrawable
    {
        /// <summary>
        /// The spacing between the beatmap picker tab control and the strips underneath it.
        /// todo: this is really hacky but I failed to find any better way around it, requires a bit more thinking.
        /// </summary>
        public const float PICKER_STRIP_SPACING = 10f;

        [Resolved]
        private Bindable<APIBeatmap> beatmap { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(7f),
                Margin = new MarginPadding { Bottom = PICKER_STRIP_SPACING },
                Children = new Drawable[]
                {
                    new BeatmapPickerListingControl { Current = beatmap }, // todo: should it actually bind current?
                    new BeatmapPickerTabControl { Current = beatmap },
                }
            };
        }
    }
}
