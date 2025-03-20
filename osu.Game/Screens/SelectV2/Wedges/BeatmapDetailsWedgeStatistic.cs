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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDetailsWedgeStatistic : FillFlowContainer
    {
        private (LocalisableString value, LinkDetails? link)? data;

        public (LocalisableString value, LinkDetails? link)? Data
        {
            get => data;
            set
            {
                data = value;

                valueText.Clear();

                if (value.HasValue)
                {
                    string valueString = value.Value.value.ToString();

                    if (value.Value.link != null)
                        valueText.AddLink(valueString.Truncate(24), value.Value.link.Action, value.Value.link.Argument);
                    else
                        valueText.AddText(valueString.Truncate(24));
                }
                else
                {
                    valueText.AddArbitraryDrawable(new LoadingSpinner
                    {
                        Size = new Vector2(16),
                        State = { Value = Visibility.Visible },
                        Margin = new MarginPadding { Top = 4f },
                    });
                }
            }
        }

        public DateTimeOffset? Date
        {
            set
            {
                valueText.Clear();

                if (value != null)
                    valueText.AddArbitraryDrawable(new DrawableDate(value.Value, textSize: 14.4f, italic: false, weight: FontWeight.Regular));
                else
                    valueText.AddText("-");
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

                    if (total > 80)
                    {
                        string displayTag = tag.Truncate(80 - lastLength);

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

        public BeatmapDetailsWedgeStatistic(LocalisableString label)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                labelText = new OsuSpriteText
                {
                    Text = label,
                    Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.SemiBold),
                },
                valueText = new LinkFlowContainer(t => t.Font = t.Font.With(size: 14.4f, weight: FontWeight.Regular))
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 14.4f,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            labelText.Colour = colourProvider.Content1;
            valueText.Colour = colourProvider.Content2;
        }
    }
}
