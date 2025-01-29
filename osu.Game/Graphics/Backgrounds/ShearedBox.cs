// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class ShearedBox : CompositeDrawable
    {
        public bool DropShadow { get; init; } = true;

        public bool LeftPadded { get; init; }

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 10f;
            Masking = true;

            Shear = new Vector2(OsuGame.SHEAR, 0);

            if (DropShadow)
            {
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Offset = new Vector2(0f, 4f),
                    Radius = 4,
                };
            }

            if (LeftPadded)
                Margin = new MarginPadding { Left = -CornerRadius };

            InternalChild = new Box { RelativeSizeAxes = Axes.Both };
        }
    }
}
