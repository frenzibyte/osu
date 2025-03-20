// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A special type of <see cref="FillFlowContainer{T}"/> that preserves the alignment of children drawables while keeping the edges sheared.
    /// </summary>
    public partial class ShearAlignedFlowContainer : CompositeDrawable
    {
        private readonly FillFlowContainer flow;

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set
            {
                base.AutoSizeAxes = value;
                flow.RelativeSizeAxes = Axes.Both & ~value;
                flow.AutoSizeAxes = value;
            }
        }

        public FillDirection Direction
        {
            get => flow.Direction;
            set => flow.Direction = value;
        }

        public Vector2 Spacing
        {
            get => flow.Spacing;
            set => flow.Spacing = value;
        }

        public Drawable[] Children
        {
            set => flow.ChildrenEnumerable = value.Select(d => new ShearAlignedDrawable(d));
        }

        public ShearAlignedFlowContainer(Vector2 shear)
        {
            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Shear = shear,
            };
        }

        private partial class ShearAlignedDrawable : CompositeDrawable
        {
            private readonly LayoutValue layout = new LayoutValue(Invalidation.MiscGeometry);

            public ShearAlignedDrawable(Drawable d)
            {
                RelativeSizeAxes = d.RelativeSizeAxes;
                AutoSizeAxes = Axes.Both & ~d.RelativeSizeAxes;

                InternalChild = d;

                AddLayout(layout);
            }

            protected override void Update()
            {
                base.Update();

                if (!layout.IsValid)
                {
                    updateLayout();
                    layout.Validate();
                }
            }

            private void updateLayout()
            {
                float shearWidth = Parent!.Shear.X * Parent!.DrawHeight;
                float relativeY = Parent!.DrawHeight == 0 ? 0 : Y / Parent!.DrawHeight;

                Shear = -Parent!.Shear;
                Padding = new MarginPadding { Left = shearWidth * relativeY };
            }
        }
    }
}
