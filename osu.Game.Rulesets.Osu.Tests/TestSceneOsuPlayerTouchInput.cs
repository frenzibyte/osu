// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneOsuPlayerTouchInput : TestSceneOsuPlayer
    {
        protected override bool HasCustomSteps => true;

        [Test]
        public void TestPressBeforeLoad()
        {
            AddStep("begin touch", () => InputManager.BeginTouch(new Touch(TouchSource.Touch1, InputManager.ScreenSpaceDrawQuad.Centre)));

            CreateTest();

            OsuInputManager inputManager = null!;
            OsuTouchInputMapper touchInputMapper = null!;

            AddStep("get input manager", () => inputManager = Player.DrawableRuleset.Playfield.FindClosestParent<OsuInputManager>()!);
            AddStep("get touch input mapper", () => touchInputMapper = inputManager.ChildrenOfType<OsuTouchInputMapper>().Single());
            AddAssert("touch tracked", () => touchInputMapper.TrackedTouches, () => Has.Count.EqualTo(1));

            AddAssert("touch is correct", () => touchInputMapper.TrackedTouches[0].DirectTouch == false &&
                                                touchInputMapper.TrackedTouches[0].Action == OsuAction.LeftButton &&
                                                touchInputMapper.TrackedTouches[0].Source == TouchSource.Touch1 &&
                                                touchInputMapper.TrackedTouches[0].DistanceTravelled == 0f);

            AddStep("end touch", () => InputManager.EndTouch(new Touch(TouchSource.Touch1, InputManager.ScreenSpaceDrawQuad.Centre)));
            AddAssert("touch released", () => touchInputMapper.TrackedTouches, () => Has.Count.Zero);
        }
    }
}
