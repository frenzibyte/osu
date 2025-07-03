// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonManiaComboCounter : ArgonComboCounter, IHasSkinEditorProperties
    {
        protected override bool DisplayXSymbol => false;

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; } = null!;

        private IBindable<ScrollingDirection> direction = null!;

        [SettingSource("Adjust position based on scrolling direction",
            "Whether this should be positioned relative to the hit area rather than being fixed on the screen. If enabled, anchor selection is limited and controlled based on the scrolling direction.")]
        public BindableBool ScrollDirectionAdjustment { get; } = new BindableBool(true);

        public bool AllowsClosestAnchor { get; set; }

        public Anchor[]? AllowedAnchors { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => updateAnchor());
            ScrollDirectionAdjustment.BindValueChanged(_ => updateAnchor());

            // two schedules are required so that updateAnchor is executed in the next frame,
            // which is when the combo counter receives its Y position by the default layout in ArgonManiaSkinTransformer.
            Schedule(() => Schedule(updateAnchor));
        }

        private void updateAnchor()
        {
            if (!ScrollDirectionAdjustment.Value)
            {
                AllowsClosestAnchor = true;
                AllowedAnchors = null;
                return;
            }

            // the logic of flipping the position of the combo counter w.r.t. the direction does not work with "Closest" anchor,
            // because it always forces the anchor to be top or bottom based on scrolling direction.
            AllowsClosestAnchor = false;

            if (direction.Value == ScrollingDirection.Down)
                AllowedAnchors = new[] { Anchor.TopLeft, Anchor.TopCentre, Anchor.TopRight };
            else
                AllowedAnchors = new[] { Anchor.BottomLeft, Anchor.BottomCentre, Anchor.BottomRight };

            UsesFixedAnchor = true;
            Anchor &= ~(Anchor.y0 | Anchor.y1 | Anchor.y2);
            Anchor |= direction.Value == ScrollingDirection.Up ? Anchor.y2 : Anchor.y0;

            // change the sign of the Y coordinate in line with the scrolling direction.
            // i.e. if the user changes direction from down to up, the anchor is changed from top to bottom, and the Y is flipped from positive to negative here.
            Y = Math.Abs(Y) * (direction.Value == ScrollingDirection.Up ? -1 : 1);
        }
    }
}
