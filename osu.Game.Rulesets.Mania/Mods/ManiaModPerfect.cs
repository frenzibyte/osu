// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModPerfect : ModPerfect
    {
        public override AppliedFailResult ApplyToFailure(JudgementResult result)
        {
            if (!isRelevantResult(result.Judgement.MinResult) && !isRelevantResult(result.Judgement.MaxResult) && !isRelevantResult(result.Type))
                return AppliedFailResult.Nothing;

            // Mania allows imperfect "Great" hits without failing.
            if (result.Judgement.MaxResult == HitResult.Perfect)
                return result.Type < HitResult.Great ? AppliedFailResult.TriggerFail : AppliedFailResult.Nothing;

            return result.Type != result.Judgement.MaxResult ? AppliedFailResult.TriggerFail : AppliedFailResult.Nothing;
        }

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}
