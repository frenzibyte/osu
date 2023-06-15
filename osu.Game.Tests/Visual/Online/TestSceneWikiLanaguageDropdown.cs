// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.Wiki;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneWikiLanaguageDropdown : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() => new WikiLanguageDropdown
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Y = 200,
        };
    }
}
