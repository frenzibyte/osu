// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.SelectV2.Leaderboards;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneLeaderboardScopeControl : ThemeComparisonTestScene
    {
        public TestSceneLeaderboardScopeControl()
            : base(false)
        {
        }

        protected override Drawable CreateContent()
        {
            return new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(0.5f),
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.3f),
                    },
                    new LeaderboardScopeControl
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            };
        }
    }
}
