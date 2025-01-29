// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.SelectV2.Wedge;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultyNameContent : SongSelectComponentsTestScene
    {
        private DifficultyNameContent difficultyNameContent = null!;

        [Test]
        public void TestLocalBeatmap()
        {
            AddStep("set component", () => Child = difficultyNameContent = new DifficultyNameContent());

            AddAssert("difficulty name is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is not set", () => LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<OsuHoverContainer>().Single().ChildrenOfType<OsuSpriteText>().Single().Text));

            AddStep("set data", () =>
            {
                difficultyNameContent.Value = new DifficultyNameContent.Data
                {
                    DifficultyName = "really long difficulty name that gets truncated",
                    Mapper = new APIUser { Username = "really long username that is autosized" }
                };
            });

            AddAssert("difficulty name is set", () => !LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<TruncatingSpriteText>().Single().Text));
            AddAssert("author is set", () => !LocalisableString.IsNullOrEmpty(difficultyNameContent.ChildrenOfType<OsuHoverContainer>().Single().ChildrenOfType<OsuSpriteText>().Single().Text));
        }
    }
}
