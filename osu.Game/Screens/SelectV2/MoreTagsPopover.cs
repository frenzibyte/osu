// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.SelectV2
{
    public partial class MoreTagsPopover : OsuPopover
    {
        private readonly string[] tags;

        public MoreTagsPopover(string[] tags)
        {
            this.tags = tags;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LinkFlowContainer textFlow;

            Child = textFlow = new LinkFlowContainer(t =>
            {
                t.Font = t.Font.With(size: 14.4f, weight: FontWeight.Bold);
            })
            {
                Width = 200,
                AutoSizeAxes = Axes.Y,
            };

            // foreach (string tag in tags)
            //     textFlow.AddArbitraryDrawable(new TagButton(tag));
            foreach (string tag in tags)
            {
                textFlow.AddLink(tag, LinkAction.SearchBeatmapSet, tag);
                textFlow.AddText(" ");
            }
        }
    }
}
