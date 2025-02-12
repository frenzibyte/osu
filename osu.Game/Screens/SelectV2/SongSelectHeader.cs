// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class SongSelectHeader : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = 176f;

            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 4f,
                Colour = Color4.Black.Opacity(0.25f),
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Dark6,
                },
                new SongSelectHeaderTitle
                {
                    Position = new Vector2(62, 36f),
                },
                new FillFlowContainer
                {
                    Position = new Vector2(-45f, 43f),
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new ShearedSearchTextBox
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Width = 570,
                        },
                        new RangeShearedSlider
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Width = 520f,
                            LowerBound = new BindableDouble(0f) { MinValue = 0, MaxValue = 10 },
                            UpperBound = new BindableDouble(10f) { MinValue = 0, MaxValue = 10 },
                        },
                    },
                },
            };
        }
    }
}
