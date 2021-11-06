// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    /// <summary>
    /// A <see cref="BeatmapMetadataItem"/> with a defined <see cref="LinkAction"/> accepting link arguments to build the link with and display.
    /// </summary>
    public class BeatmapLinkedMetadataItem : BeatmapMetadataItem
    {
        private readonly LinkAction action;
        private readonly string separator;

        public string Value
        {
            set => Values = new[] { value };
        }

        public string[] Values
        {
            set
            {
                linkFlow.Clear();

                if (value.Length == 0)
                    return;

                for (int i = 0; i < value.Length - 1; i++)
                {
                    linkFlow.AddLink(value[i], action, value[i]);
                    linkFlow.AddText(separator);
                }

                linkFlow.AddLink(value[^1], action, value[^1]);
            }
        }

        private readonly LinkFlowContainer linkFlow;

        public BeatmapLinkedMetadataItem(string name, LinkAction action, string separator = " ", float? height = null)
            : base(name)
        {
            this.action = action;
            this.separator = separator;

            LabelText.Width = 80f;

            // todo: pending truncating support in TextFlowContainer.
            MainContainer.Add(linkFlow = new LinkFlowContainer(s => s.Font = OsuFont.Default.With(size: 12f, weight: FontWeight.Bold))
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Padding = new MarginPadding { Left = LabelText.Width },
                RelativeSizeAxes = Axes.X,
            });

            if (height != null)
                linkFlow.Height = (float)height;
            else
                linkFlow.AutoSizeAxes = Axes.Y;
        }
    }
}
