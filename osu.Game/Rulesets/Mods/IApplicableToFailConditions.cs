// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can override existing fail conditions and/or block fails.
    /// </summary>
    public interface IApplicableToFailConditions : IApplicableMod
    {
        /// <summary>
        /// Invoked from this mod to arbitrarily trigger a failure in gameplay.
        /// This is set by the <see cref="HealthProcessor"/> that received this mod.
        /// </summary>
        Action? TriggerFailure { set; }

        /// <summary>
        /// Whether we want to restart on fail. Only effective if <see cref="ApplyToFailure"/> returns <see cref="AppliedFailResult.TriggerFail"/>.
        /// </summary>
        bool RestartOnFail { get; }

        /// <summary>
        /// Determines whether <paramref name="result"/> should trigger a failure. Called every time a
        /// judgement is applied to <see cref="HealthProcessor"/>.
        /// </summary>
        /// <param name="result">The latest <see cref="JudgementResult"/>.</param>
        /// <returns>Whether the fail condition has been met.</returns>
        /// <remarks>
        /// This method should only be used to trigger failures based on <paramref name="result"/>.
        /// Using outside values to evaluate failure may introduce event ordering discrepancies, use <see cref="TriggerFailure"/> instead.
        /// </remarks>
        AppliedFailResult ApplyToFailure(JudgementResult result);
    }

    public enum AppliedFailResult
    {
        /// <summary>
        /// Do nothing, if there are other mods that may trigger or block fail, let them do so.
        /// This should be used when the fail block/trigger conditions provided by the mod do not apply to the current state of gameplay, to allow for other mods to still apply their own conditions.
        /// </summary>
        Nothing,

        /// <summary>
        /// Block fail regardless of other mods.
        /// </summary>
        BlockFail,

        /// <summary>
        /// Trigger fail regardless of other mods.
        /// </summary>
        TriggerFail
    }
}
