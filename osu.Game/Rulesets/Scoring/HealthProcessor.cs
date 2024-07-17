// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    public abstract partial class HealthProcessor : JudgementProcessor
    {
        /// <summary>
        /// Invoked when the <see cref="HealthProcessor"/> is in a failed state.
        /// </summary>
        public event Func<bool>? Failed;

        /// <summary>
        /// The current health.
        /// </summary>
        public readonly BindableDouble Health = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Whether this <see cref="HealthProcessor"/> has already triggered the failed state.
        /// </summary>
        public bool HasFailed { get; private set; }

        /// <summary>
        /// The current selected mods
        /// </summary>
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// The mod which triggered failure, if <see cref="HasFailed"/> is true and failure was triggered by a mod.
        /// </summary>
        public IApplicableToFailConditions? ModTriggeringFailure { get; private set; }

        protected HealthProcessor()
        {
            Mods.ValueChanged += mods =>
            {
                foreach (var m in mods.NewValue.OfType<IApplicableToFailConditions>())
                    m.TriggerFailure = () => TriggerFailure(m);
            };
        }

        /// <summary>
        /// Immediately triggers a failure for this HealthProcessor.
        /// </summary>
        /// <param name="triggeringMod">The mod triggering this failure, if it exists.</param>
        public void TriggerFailure(IApplicableToFailConditions? triggeringMod = null)
        {
            if (HasFailed)
                return;

            if (Failed?.Invoke() == false)
                return;

            HasFailed = true;
            ModTriggeringFailure = triggeringMod;
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            result.HealthAtJudgement = Health.Value;
            result.FailedAtJudgement = HasFailed;

            if (HasFailed)
                return;

            Health.Value += GetHealthIncreaseFor(result);

            if (meetsAnyFailCondition(result, out var triggeringMod))
                TriggerFailure(triggeringMod);
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            Health.Value = result.HealthAtJudgement;

            // Todo: Revert HasFailed state with proper player support
        }

        /// <summary>
        /// Retrieves the health increase for a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/>.</param>
        /// <returns>The health increase.</returns>
        protected virtual double GetHealthIncreaseFor(JudgementResult result) => result.HealthIncrease;

        /// <summary>
        /// Checks whether the default conditions for failing are met.
        /// </summary>
        /// <returns><see langword="true"/> if failure should be invoked.</returns>
        protected virtual bool CheckDefaultFailCondition(JudgementResult result) => Precision.AlmostBigger(Health.MinValue, Health.Value);

        /// <summary>
        /// Whether the current state of <see cref="HealthProcessor"/> or the provided <paramref name="result"/> meets any fail condition.
        /// </summary>
        /// <param name="result">The judgement result.</param>
        /// <param name="triggeringMod">The mod which triggered failure, if this method returned <c>true</c>.</param>
        /// <returns>Whether failure should be triggered.</returns>
        private bool meetsAnyFailCondition(JudgementResult result, out IApplicableToFailConditions? triggeringMod)
        {
            triggeringMod = null;

            foreach (var mod in Mods.Value.OfType<IApplicableToFailConditions>().OrderByDescending(f => f.RestartOnFail))
            {
                var failResult = mod.ApplyToFailure(result);

                switch (failResult)
                {
                    case AppliedFailResult.BlockFail:
                        return false;

                    case AppliedFailResult.TriggerFail:
                        triggeringMod = mod;
                        return true;
                }
            }

            return CheckDefaultFailCondition(result);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 1;
            HasFailed = false;
        }
    }
}
