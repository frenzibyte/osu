// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    public class BeatmapMetadataPill : CompositeDrawable
    {
        private (string label, string value)[] values = Array.Empty<(string, string)>();

        public (string label, string value)[] Values
        {
            set
            {
                if (value == values)
                    return;

                values = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private readonly Box background;
        private readonly OsuTextFlowContainer textFlow;

        public BeatmapMetadataPill(Action<SpriteText>? textCreationParameters = null)
        {
            InternalChild = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    textFlow = new OsuTextFlowContainer(s =>
                    {
                        s.Font = s.Font.With(size: 12, weight: FontWeight.SemiBold);
                        textCreationParameters?.Invoke(s);
                    })
                    {
                        TextAnchor = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Padding = new MarginPadding { Top = 2f }, // todo: fuck... (...huh?)
                    }
                }
            };
        }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            textFlow.Clear();

            for (int i = 0; i < values.Length; i++)
            {
                textFlow.AddText(values[i].label, s => s.Colour = colourProvider.Content1);
                textFlow.AddArbitraryDrawable(Empty().With(s => s.Width = 7.5f));
                textFlow.AddText(values[i].value ?? "-", s =>
                {
                    s.Colour = colourProvider.Content2;
                    s.Font = s.Font.With(weight: FontWeight.Bold);
                });

                if (i < values.Length - 1)
                    textFlow.AddArbitraryDrawable(Empty().With(s => s.Width = 20));
            }
        }
    }
}
