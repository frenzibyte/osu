// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    public class BeatmapDateTimeMetadataItem : BeatmapMetadataItem
    {
        private DateTimeOffset? date;

        public DateTimeOffset? Value
        {
            get => date;
            set
            {
                if (value == date)
                    return;

                date = value;

                if (value == null)
                {
                    valueText.Text = "-";
                    return;
                }

                valueText.Text = value.Value.ToString("MMM d, yyyy");
            }
        }

        private readonly DateSpriteText valueText;

        public BeatmapDateTimeMetadataItem(string name)
            : base(name)
        {
            LabelText.Width = 80f;

            MainContainer.Add(new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Left = 80f },
                Child = valueText = new DateSpriteText(this)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                    Text = "-",
                }
            });
        }

        private class DateSpriteText : OsuSpriteText, IHasCustomTooltip
        {
            private readonly BeatmapDateTimeMetadataItem item;

            public DateSpriteText(BeatmapDateTimeMetadataItem item)
            {
                this.item = item;
            }

            public ITooltip GetCustomTooltip() => new DateTooltip();

            public object? TooltipContent => item.Value;
        }
    }
}
