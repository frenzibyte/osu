// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// Displays contents in a "pill".
    /// </summary>
    public class PillContainer : Container
    {
        private readonly bool autoSize;

        public readonly Drawable Background;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        private readonly Container gridContainer;
        private readonly GridContainer grid;

        public MarginPadding ContentPadding
        {
            get => gridContainer.Padding;
            set
            {
                gridContainer.Padding = value;

                if (autoSize)
                {
                    grid.ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize, minSize: 80 - value.TotalHorizontal)
                    };
                }
            }
        }

        public PillContainer(bool autoSize)
        {
            this.autoSize = autoSize;

            Container circularContainer, contentContainer;

            InternalChild = circularContainer = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                Children = new[]
                {
                    Background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    gridContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = grid = new GridContainer
                        {
                            Content = new[]
                            {
                                new[]
                                {
                                    contentContainer = new Container
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Padding = new MarginPadding { Bottom = 2 },
                                        Child = content = new Container
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            if (autoSize)
            {
                AutoSizeAxes = Axes.X;
                Height = 16;

                circularContainer.AutoSizeAxes = Axes.X;
                circularContainer.RelativeSizeAxes = Axes.Y;

                gridContainer.AutoSizeAxes = Axes.Both;
                grid.AutoSizeAxes = Axes.Both;
                contentContainer.AutoSizeAxes = Axes.Both;
            }
            else
            {
                RelativeSizeAxes = Axes.Both;

                circularContainer.RelativeSizeAxes = Axes.Both;

                gridContainer.RelativeSizeAxes = Axes.Both;
                grid.RelativeSizeAxes = Axes.Both;
                contentContainer.RelativeSizeAxes = Axes.Both;
            }

            ContentPadding = new MarginPadding { Horizontal = 8f };
        }
    }
}
