// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;

namespace osu.Game.Graphics.UserInterface
{
    public class BarGraph : FillFlowContainer<Bar>
    {
        private readonly Action<(Bar bar, int index, int count)> parameters;

        /// <summary>
        /// Manually sets the max value, if null <see cref="Enumerable.Max(IEnumerable{float})"/> is used instead.
        /// </summary>
        public float? MaxValue { get; set; }

        private BarDirection direction = BarDirection.BottomToTop;

        public new BarDirection Direction
        {
            get => direction;
            set
            {
                direction = value;
                base.Direction = direction.HasFlagFast(BarDirection.Horizontal) ? FillDirection.Vertical : FillDirection.Horizontal;

                updateBarsLayout();
            }
        }

        public new Vector2 Spacing
        {
            get => base.Spacing;
            set
            {
                base.Spacing = value;
                updateBarsLayout();
            }
        }

        /// <summary>
        /// A list of floats that defines the length of each <see cref="Bar"/>
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                List<Bar> bars = Children.ToList();

                foreach (var bar in value.Select((length, index) => new { Index = index, Value = length, Bar = bars.Count > index ? bars[index] : null }))
                {
                    float length = MaxValue ?? value.Max();
                    if (length != 0)
                        length = bar.Value / length;

                    if (bar.Bar != null)
                    {
                        bar.Bar.Length = length;
                        parameters?.Invoke((bar.Bar, bar.Index, value.Count()));
                    }
                    else
                    {
                        var newBar = new Bar { RelativeSizeAxes = Axes.Both };

                        parameters?.Invoke((newBar, bar.Index, value.Count()));
                        Add(newBar);

                        newBar.Length = length;
                    }
                }

                //I'm using ToList() here because Where() returns an Enumerable which can change it's elements afterwards
                RemoveRange(Children.Where((bar, index) => index >= value.Count()).ToList());

                updateBarsLayout();
            }
        }

        public BarGraph(Action<(Bar bar, int index, int count)> parameters = null)
        {
            this.parameters = parameters;
        }

        private void updateBarsLayout()
        {
            float relativeSpacing = direction.HasFlagFast(BarDirection.Horizontal) ? (Spacing.Y / DrawHeight) : (Spacing.X / DrawWidth);

            for (int i = 0; i < Children.Count; i++)
            {
                Bar bar = Children[i];
                bool last = i == Children.Count - 1;

                float size = 1.0f / Children.Count - (last ? 0 : relativeSpacing);

                bar.Size = direction.HasFlagFast(BarDirection.Horizontal) ? new Vector2(1, size) : new Vector2(size, 1);
                bar.Direction = direction;
            }
        }
    }
}
