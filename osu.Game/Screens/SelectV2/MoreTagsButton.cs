// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Screens.SelectV2
{
    public partial class MoreTagsButton : TagButton, IHasPopover
    {
        private readonly string[] tags;

        public MoreTagsButton(string[] tags)
            : base("...")
        {
            this.tags = tags;

            Action = this.ShowPopover;
        }

        public Popover GetPopover() => new MoreTagsPopover(tags);
    }
}
