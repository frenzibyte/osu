// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedgeStatistic : CompositeDrawable
    {
        public (LocalisableString value, LinkDetails link) Value
        {
            set
            {
                valueText.Clear();
                valueText.AddLink(value.value, value.link.Action, value.link.Argument);
            }
        }

        public DateTimeOffset Date
        {
            set
            {
                valueText.Clear();
                valueText.AddArbitraryDrawable(new DrawableDate(value, textSize: 14.4f, italic: false, weight: FontWeight.Bold));
            }
        }

        public string[] Tags
        {
            set
            {
                valueText.Clear();
                int total = 0;

                foreach (string tag in value)
                {
                    int lastLength = total;
                    total += tag.Length + 1;

                    if (total > 90)
                    {
                        string displayTag = tag.Truncate(90 - lastLength);

                        valueText.AddLink(displayTag, LinkAction.SearchBeatmapSet, tag, displayTag != tag ? tag : null);
                        valueText.AddText(" ");
                        valueText.AddArbitraryDrawable(new MoreTagsButton(value));
                        break;
                    }

                    valueText.AddLink(tag, LinkAction.SearchBeatmapSet, tag);
                    valueText.AddText(" ");
                }
            }
        }

        private readonly OsuSpriteText labelText;
        private readonly LinkFlowContainer valueText;

        public BeatmapContentWedgeStatistic(LocalisableString label, float? height = null)
        {
            RelativeSizeAxes = Axes.X;

            if (height == null)
                AutoSizeAxes = Axes.Y;
            else
                Height = height.Value;

            InternalChildren = new Drawable[]
            {
                labelText = new OsuSpriteText
                {
                    Width = 80,
                    Text = label,
                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                },
                valueText = new LinkFlowContainer(t => t.Font = t.Font.With(size: 14.4f, weight: FontWeight.Bold))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = 80f },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            labelText.Colour = colourProvider.Content1;
        }
    }
}
