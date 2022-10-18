// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Game.Graphics.Sprites;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSketch : OsuGridTestScene
    {
        public TestSceneSketch()
            : base(1, 3)
        {
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Cell(0).Child = createProvider("master", new Video(TestResources.GetStore().GetStream("Resources/Videos/master.mp4"))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
            });
            Cell(1).Child = createProvider("PR", new Video(TestResources.GetStore().GetStream("Resources/Videos/pr.mp4"))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
            });
            Cell(2).Child = createProvider("stable", new Video(TestResources.GetStore().GetStream("Resources/Videos/stable.mp4"))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
            });
        });

        private Drawable createProvider(string name, Drawable content) => new Container
        {
            RelativeSizeAxes = Axes.Both,
            BorderColour = Color4.White,
            BorderThickness = 5,
            Masking = true,
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = content,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = name,
                    Scale = new Vector2(3f),
                    Padding = new MarginPadding(5),
                },
            }
        };
    }
}
