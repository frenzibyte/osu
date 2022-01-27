// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneExpandingControlContainer : OsuTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Test]
        public void TestButton()
        {
            ExpandingControlContainer expandingControlContainer = null;

            AddStep("create content", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeaFoam
                    },
                    expandingControlContainer = new ExpandingControlContainer
                    {
                        Padding = new MarginPadding(10),
                        CollapsedText = "C. B.",
                        ExpandedContent = new RoundedButton
                        {
                            Text = "Expanded Button",
                            Width = 300,
                            Action = () => { }
                        }
                    }
                }
            });
            AddToggleStep("force expanded", expanded =>
            {
                if (expandingControlContainer != null)
                    expandingControlContainer.ForceExpanded.Value = expanded;
            });
        }

        [Test]
        public void TestSlider()
        {
            ExpandingControlContainer expandingControlContainer = null;

            AddStep("create content", () => Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.Centre,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new SettingsToolboxGroup("sliders")
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            expandingControlContainer = new ExpandingControlContainer
                            {
                                CollapsedText = "C. S.",
                                ExpandedContent = new Container
                                {
                                    AutoSizeAxes = Axes.Y,
                                    Width = 200,
                                    Child = new SettingsSlider<float>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        LabelText = "Expanded Slider",
                                        Current = new BindableNumber<float>
                                        {
                                            MinValue = 1,
                                            MaxValue = 10,
                                            Precision = 0.1f,
                                            Default = 5,
                                            Value = 5
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            AddToggleStep("force expanded", expanded =>
            {
                if (expandingControlContainer != null)
                    expandingControlContainer.ForceExpanded.Value = expanded;
            });
        }
    }
}
