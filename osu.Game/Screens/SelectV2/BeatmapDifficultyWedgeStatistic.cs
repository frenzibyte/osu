// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDifficultyWedgeStatistic : CompositeDrawable, IHasAccentColour
    {
        public (LocalisableString text, float current, float maximum) Value
        {
            set
            {
                bar.Width = value.current / value.maximum;
                valueText.Text = value.text;
            }
        }

        private readonly Circle bar;
        private readonly OsuSpriteText labelText;
        private readonly OsuSpriteText valueText;

        public Color4 AccentColour
        {
            get => bar.Colour;
            set => bar.Colour = value;
        }

        public BeatmapDifficultyWedgeStatistic(LocalisableString label)
        {
            Width = 65;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            new Circle
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 2.5f,
                                Colour = Color4.Black,
                                Masking = true,
                                CornerRadius = 1f,
                            },
                            bar = new Circle
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0f,
                                Height = 2.5f,
                                Masking = true,
                                CornerRadius = 1f,
                            },
                        },
                    },
                    labelText = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 2f },
                        Text = label,
                        Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    },
                    valueText = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 0f },
                        Font = OsuFont.Torus.With(size: 20f, weight: FontWeight.Regular),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            labelText.Colour = colourProvider.Content2;
            valueText.Colour = colourProvider.Content1;
        }
    }
}
