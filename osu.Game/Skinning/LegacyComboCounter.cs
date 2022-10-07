// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Displays a legacy combo counter based on the selected skin.
    /// For non-legacy rulesets, the default osu! combo counter is displayed.
    /// </summary>
    /// <remarks>
    /// This internally works by adding all legacy combo counter variants for serialisation, while only one is displayed based on the selected ruleset.
    /// Container inheritance is required in order for the underlying <see cref="LegacyRulesetComboCounter"/> of this component to be included in the SkinnableInfo serialisation.
    /// </remarks>
    public class LegacyComboCounter : Container
    {
        public LegacyComboCounter()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IRulesetStore rulesets)
        {
            foreach (var ruleset in rulesets.AvailableRulesets.Take(ILegacyRuleset.MAX_LEGACY_RULESET_ID + 1))
            {
                var legacyRuleset = (ILegacyRuleset)ruleset.CreateInstance();
                AddInternal(legacyRuleset.CreateLegacyComboCounter());
            }
        }
    }
}
