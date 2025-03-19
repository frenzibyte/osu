// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearAlignedFlowContainer : OsuTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Add(new ShearAlignedFlowContainer(new Vector2(OsuGame.SHEAR, 0))
            {
                Position = new Vector2(400, 200),
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.2f, 0.3f),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 10f),
                Children = new Drawable[]
                {
                    new ShearedBox("Primary", OsuColour.Gray(0.4f))
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                    },
                    new ShearedBox("Secondary", OsuColour.Gray(0.3f))
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                    },
                    new ShearedBox("Teritary", OsuColour.Gray(0.2f))
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                    },
                }
            });
        });

        public partial class ShearedBox : Container
        {
            private readonly string text;
            private readonly Color4 boxColour;

            public ShearedBox(string text, Color4 boxColour)
            {
                this.text = text;
                this.boxColour = boxColour;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Shear = new Vector2(OsuGame.SHEAR, 0);

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = boxColour,
                };
                AddInternal(new OsuSpriteText
                {
                    Text = text,
                    Colour = Color4.White,
                    Shear = -new Vector2(OsuGame.SHEAR, 0),
                    Margin = new MarginPadding { Left = 50 },
                });
            }
        }
    }
}
