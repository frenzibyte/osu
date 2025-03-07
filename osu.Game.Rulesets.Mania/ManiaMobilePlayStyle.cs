// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Mania
{
    public enum ManiaMobilePlayStyle
    {
        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.MobilePlayStylePortrait))]
        Portrait,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.MobilePlayStyleLandscapeExtended))]
        LandscapeExtendedColumns,

        [LocalisableDescription(typeof(RulesetSettingsStrings), nameof(RulesetSettingsStrings.MobilePlayStyleLandscapeTouch))]
        LandscapeTouchControls,
    }
}
