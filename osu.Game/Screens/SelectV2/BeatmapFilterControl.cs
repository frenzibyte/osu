// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Filter;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapFilterControl : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuConfigManager config)
        {
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = -10f, Right = -40f, Left = -80f },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f,
                                Colour = ColourInfo.GradientHorizontal(colourProvider.Background4.Opacity(0f), colourProvider.Background4.Opacity(1f)),
                            },
                            new Box
                            {
                                RelativePositionAxes = Axes.X,
                                X = 0.2f,
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.8f,
                                Colour = colourProvider.Background4,
                            },
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 8f),
                    Padding = new MarginPadding { Vertical = 15f, Right = 15f, Left = 20f },
                    Shear = new Vector2(OsuGame.SHEAR, 0),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
                            Child = new ShearedSearchTextBox
                            {
                                RelativeSizeAxes = Axes.X,
                                Scale = new Vector2(0.875f),
                                Width = 1 / 0.875f,
                            },
                        },
                        new ShearedDifficultyRangeSlider
                        {
                            RelativeSizeAxes = Axes.X,
                            LowerBound = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                            UpperBound = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                            MinRange = 0.1f,
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
                            Scale = new Vector2(0.875f),
                            Width = 1 / 0.875f,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Shear = -new Vector2(OsuGame.SHEAR, 0),
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new Container
                                    {
                                        Size = new Vector2(210, 30),
                                        Scale = new Vector2(0.875f),
                                        Child = new ShearedDropdown<SortMode>("Sort by")
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = Enum.GetValues<SortMode>(),
                                        },
                                    },
                                    Empty(),
                                    new Container
                                    {
                                        Size = new Vector2(220, 30),
                                        Scale = new Vector2(0.875f),
                                        Child = new ShearedDropdown<GroupMode>("Group by")
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Items = Enum.GetValues<GroupMode>(),
                                        },
                                    },
                                    Empty(),
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 1 / 0.875f,
                                        Height = 30f,
                                        Scale = new Vector2(0.875f),
                                        Child = new ShearedDropdown<string>("Collection")
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                    },
                                }
                            }
                        },
                    },
                }
            };
        }
    }
}
