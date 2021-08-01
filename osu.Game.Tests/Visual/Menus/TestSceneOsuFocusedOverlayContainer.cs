// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneOsuFocusedOverlayContainer : OsuTestScene
    {
        private TestOverlay overlay;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            LoadComponentAsync(overlay = new TestOverlay
            {
                State = { Value = Visibility.Visible },
            }, c =>
            {
                Child = c;
            });
        });

        [Test]
        public void TestDisabledOverlayActivation([Values] bool finishLoad)
        {
            if (finishLoad)
                AddStep("finish load", () => overlay.AllowLoad.Set());

            AddAssert("overlay shown", () => overlay.State.Value == Visibility.Visible);

            AddStep("disable overlay activation", () => overlay.OverlayActivationMode.Value = OverlayActivation.Disabled);
            AddAssert("overlay hidden", () => overlay.State.Value == Visibility.Hidden);

            AddStep("attempt show overlay", () => overlay.State.Value = Visibility.Visible);
            AddAssert("overlay hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        private class TestOverlay : WaveOverlayContainer
        {
            public Bindable<OverlayActivation> OverlayActivationMode => (Bindable<OverlayActivation>)OverlayState.OverlayActivationMode;

            public readonly ManualResetEventSlim AllowLoad = new ManualResetEventSlim(false);

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (!AllowLoad.Wait(TimeSpan.FromSeconds(10)))
                    throw new TimeoutException();

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(0.5f);

                Waves.FirstWaveColour = colours.Gray8;
                Waves.SecondWaveColour = colours.Gray7;
                Waves.ThirdWaveColour = colours.Gray4;
                Waves.FourthWaveColour = colours.Gray3;

                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray2,
                };
            }
        }
    }
}
