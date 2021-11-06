// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public class CircularBar : Bar
    {
        protected override Drawable CreateBackground() => new Circle();
        protected override Drawable CreateBar() => new Circle();
    }
}
