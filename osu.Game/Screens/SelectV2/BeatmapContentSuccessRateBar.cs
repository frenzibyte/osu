// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentSuccessRateBar : CompositeDrawable
    {
        private readonly OsuSpriteText valueText;
        private readonly Circle backgroundBar;
        private readonly Circle valueBar;

        public float Value
        {
            set
            {
                valueText.Text = value.ToLocalisableString(@"0.##%");
                valueBar.Width = value;
            }
        }

        public BeatmapContentSuccessRateBar()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 40f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 2f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = BeatmapsetsStrings.ShowInfoSuccessRate,
                            Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                        },
                        valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Regular),
                            Text = "0.00%",
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new[]
                            {
                                backgroundBar = new Circle
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 4f,
                                },
                                valueBar = new Circle
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0f,
                                    Height = 4f,
                                },
                            },
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            backgroundBar.Colour = colourProvider.Background6;
            valueBar.Colour = colours.Lime1;
        }
    }
}
