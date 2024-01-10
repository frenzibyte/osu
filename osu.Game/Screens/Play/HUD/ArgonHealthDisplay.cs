// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthDisplay : HealthDisplay, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        private readonly BindableFloat barLength = new BindableFloat();

        [SettingSource("Bar height")]
        public BindableFloat BarHeight { get; } = new BindableFloat(20)
        {
            MinValue = 0,
            MaxValue = 64,
            Precision = 1
        };

        [SettingSource("Use relative size")]
        public BindableBool UseRelativeSize { get; } = new BindableBool(true);

        private readonly LayoutValue barLengthLayout = new LayoutValue(Invalidation.DrawSize);

        private ArgonHealthMainBar healthBar = null!;
        private ArgonHealthSecondaryBar secondaryBar = null!;

        public ArgonHealthDisplay()
        {
            AddLayout(barLengthLayout);

            // sane default width specification.
            // this only matters if the health display isn't part of the default skin
            // (in which case width will be set to 300 via `ArgonSkin.GetDrawableComponent()`),
            // and if the user hasn't applied their own modifications
            // (which are applied via `SerialisedDrawableInfo.ApplySerialisedInfo()`).
            Width = 0.98f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new ArgonHealthBackgroundBar
                    {
                        BarLength = { BindTarget = barLength },
                        BarHeight = { BindTarget = BarHeight },
                    },
                    secondaryBar = new ArgonHealthSecondaryBar
                    {
                        BarLength = { BindTarget = barLength },
                        BarHeight = { BindTarget = BarHeight },
                    },
                    healthBar = new ArgonHealthMainBar
                    {
                        AutoSizeAxes = Axes.None,
                        RelativeSizeAxes = Axes.Both,
                        BarLength = { BindTarget = barLength },
                        BarHeight = { BindTarget = BarHeight },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // we're about to set `RelativeSizeAxes` depending on the value of `UseRelativeSize`.
            // setting `RelativeSizeAxes` internally transforms absolute sizing to relative and back to keep the size the same,
            // but that is not what we want in this case, since the width at this point is valid in the *target* sizing mode.
            // to counteract this, store the numerical value here, and restore it after setting the correct initial relative sizing axes.
            float previousWidth = Width;
            UseRelativeSize.BindValueChanged(v => RelativeSizeAxes = v.NewValue ? Axes.X : Axes.None, true);
            Width = previousWidth;
        }

        protected override void Update()
        {
            base.Update();

            secondaryBar.EndValue = Current.Value;
            healthBar.EndValue = Current.Value;

            if (!barLengthLayout.IsValid)
            {
                const float padding = ArgonHealthBar.PATH_RADIUS * 2;

                float usableWidth = DrawWidth - padding;

                if (usableWidth < 0) enforceMinimumWidth();

                barLength.Value = DrawWidth;
                barLengthLayout.Validate();

                void enforceMinimumWidth()
                {
                    // Switch to absolute in order to be able to define a minimum width.
                    // Then switch back is required. Framework will handle the conversion for us.
                    Axes relativeAxes = RelativeSizeAxes;
                    RelativeSizeAxes = Axes.None;

                    Width = padding;

                    RelativeSizeAxes = relativeAxes;
                }
            }
        }
    }
}
