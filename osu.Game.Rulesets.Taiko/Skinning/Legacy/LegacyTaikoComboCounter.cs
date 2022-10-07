using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Legacy
{
    public class LegacyTaikoComboCounter : LegacyRulesetComboCounter
    {
        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        protected override bool ShouldBeAlive => drawableRuleset is DrawableTaikoRuleset;

        public LegacyTaikoComboCounter()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }
    }
}
