// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Represents a default (osu! ruleset) combo counter variant for legacy skins. Used for non-legacy rulesets.
    /// </summary>
    public class LegacyDefaultComboCounter : LegacyRulesetComboCounter
    {
        private const double big_pop_out_duration = 300;
        private const double small_pop_out_duration = 100;

        private ScheduledDelegate? scheduledPopOut;

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        protected override bool ShouldBeAlive => drawableRuleset == null || drawableRuleset.Ruleset.RulesetInfo.OnlineID <= 0;

        public LegacyDefaultComboCounter()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Margin = new MarginPadding(10);

            Scale = new Vector2(1.28f);

            PopOutCountText.Anchor = Anchor.BottomLeft;
            DisplayedCountText.Anchor = Anchor.BottomLeft;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            const float font_height_ratio = 0.625f;
            const float vertical_offset = 9;

            DisplayedCountText.OriginPosition = new Vector2(0, font_height_ratio * DisplayedCountText.Height + vertical_offset);
            DisplayedCountText.Position = new Vector2(0, -(1 - font_height_ratio) * DisplayedCountText.Height + vertical_offset);

            PopOutCountText.OriginPosition = new Vector2(3, font_height_ratio * PopOutCountText.Height + vertical_offset); // In stable, the bigger pop out scales a bit to the left
            PopOutCountText.Position = new Vector2(0, -(1 - font_height_ratio) * PopOutCountText.Height + vertical_offset);
        }

        protected override void IncrementCounter()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            DisplayedCountText.Show();

            PopOutCountText.Text = FormatCount(Current.Value);

            PopOutCountText.ScaleTo(1.56f)
                           .ScaleTo(1, big_pop_out_duration);

            PopOutCountText.FadeTo(0.6f)
                           .FadeOut(big_pop_out_duration);

            this.Delay(big_pop_out_duration - 140).Schedule(() =>
            {
                base.IncrementCounter();

                DisplayedCountText.ScaleTo(1).Then()
                                  .ScaleTo(1.1f, small_pop_out_duration / 2, Easing.In).Then()
                                  .ScaleTo(1, small_pop_out_duration / 2, Easing.Out);
            }, out scheduledPopOut);
        }

        protected override void RollCounterToZero()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            base.RollCounterToZero();
        }

        protected override void SetCounter()
        {
            scheduledPopOut?.Cancel();
            scheduledPopOut = null;

            base.SetCounter();
        }

        protected override string FormatCount(int count) => $@"{count}x";
    }
}
