// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSafeAreaHandling : OsuGameTestScene
    {
        private SafeAreaDefiningContainer safeAreaContainer;
        private BindableSafeArea safeArea;

        private readonly Bindable<float> safeAreaPaddingTop = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingBottom = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingLeft = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingRight = new BindableFloat { MinValue = 0, MaxValue = 200 };

        private readonly Bindable<bool> applySafeAreaConsiderations = new Bindable<bool>(true);
        private readonly Bindable<bool> visualiseNonSafeArea = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Usually this would be placed between the host and the game, but that's a bit of a pain to do with the test scene hierarchy.

            // Add is required for the container to get a size (and give out correct metrics to the usages in SafeAreaContainer).
            Add(safeAreaContainer = new SafeAreaDefiningContainer(safeArea = new BindableSafeArea())
            {
                RelativeSizeAxes = Axes.Both
            });

            // Cache is required for the test game to see the safe area.
            Dependencies.CacheAs<ISafeArea>(safeAreaContainer);

            Add(new SafeAreaVisualisation
            {
                Visualise = { BindTarget = visualiseNonSafeArea },
                RelativeSizeAxes = Axes.Both,
                Depth = float.MinValue,
            });

            safeAreaPaddingTop.BindValueChanged(_ => updateSafeArea());
            safeAreaPaddingBottom.BindValueChanged(_ => updateSafeArea());
            safeAreaPaddingLeft.BindValueChanged(_ => updateSafeArea());
            safeAreaPaddingRight.BindValueChanged(_ => updateSafeArea());
            applySafeAreaConsiderations.BindValueChanged(_ => updateSafeArea());

            AddSliderStep("top", 0, 200, 0, v => safeAreaPaddingTop.Value = v);
            AddSliderStep("bottom", 0, 200, 0, v => safeAreaPaddingBottom.Value = v);
            AddSliderStep("left", 0, 200, 0, v => safeAreaPaddingLeft.Value = v);
            AddSliderStep("right", 0, 200, 0, v => safeAreaPaddingRight.Value = v);
            AddToggleStep("apply safe area", v => applySafeAreaConsiderations.Value = v);
            AddToggleStep("visualise non-safe area", v => visualiseNonSafeArea.Value = v);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            updateSafeArea();
        }

        private void updateSafeArea()
        {
            safeArea.Value = new MarginPadding
            {
                Top = safeAreaPaddingTop.Value,
                Bottom = safeAreaPaddingBottom.Value,
                Left = safeAreaPaddingLeft.Value,
                Right = safeAreaPaddingRight.Value,
            };

            Game?.LocalConfig.SetValue(OsuSetting.SafeAreaConsiderations, applySafeAreaConsiderations.Value);
        }

        private class SafeAreaVisualisation : CompositeDrawable
        {
            public readonly BindableBool Visualise = new BindableBool(true);

            [Resolved]
            private ISafeArea safeArea { get; set; } = null!;

            private IBindable<MarginPadding> safeAreaPadding;

            private Box topBox;
            private Box bottomBox;
            private Box leftBox;
            private Box rightBox;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                InternalChildren = new[]
                {
                    topBox = new Box
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        Colour = Color4.Red.Opacity(0.2f)
                    },
                    bottomBox = new Box
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Colour = Color4.Red.Opacity(0.2f)
                    },
                    leftBox = new Box
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.Y,
                        Colour = Color4.Red.Opacity(0.2f)
                    },
                    rightBox = new Box
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Y,
                        Colour = Color4.Red.Opacity(0.2f)
                    },
                };

                safeAreaPadding = safeArea.SafeAreaPadding.GetBoundCopy();
                safeAreaPadding.BindValueChanged(v =>
                {
                    topBox.Height = v.NewValue.Top;
                    bottomBox.Height = v.NewValue.Bottom;
                    leftBox.Width = v.NewValue.Left;
                    rightBox.Width = v.NewValue.Right;
                }, true);

                Visualise.BindValueChanged(v => Alpha = v.NewValue ? 1 : 0);
            }
        }
    }
}
