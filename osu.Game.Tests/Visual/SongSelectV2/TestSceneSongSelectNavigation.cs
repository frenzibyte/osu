// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Menu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectNavigation : OsuGameTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("hook new song select", () => Game.ChildrenOfType<MainMenu>().Single().LoadNewSongSelect = true);
            AddRepeatStep("press enter", () => InputManager.Key(Key.Enter), 3);
            AddWaitStep("wait", 5);
        }

        [Test]
        public void TestStartGameplay()
        {
            AddStep("click logo", () =>
            {
                InputManager.MoveMouseTo(Game.ChildrenOfType<OsuLogo>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
