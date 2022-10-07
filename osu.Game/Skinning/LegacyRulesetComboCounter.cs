// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Skinning
{
    public abstract class LegacyRulesetComboCounter : CompositeDrawable, ISkinnableDrawable
    {
        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        private const double fade_out_duration = 100;

        /// <summary>
        /// Duration in milliseconds for the counter roll-up animation for each element.
        /// </summary>
        private const double rolling_duration = 20;

        protected readonly LegacySpriteText PopOutCountText;
        protected readonly LegacySpriteText DisplayedCountText;

        private int previousValue;

        private int displayedCount;

        private bool isRolling;

        public bool UsesFixedAnchor { get; set; }

        protected LegacyRulesetComboCounter()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new[]
            {
                PopOutCountText = new LegacySpriteText(LegacyFont.Combo)
                {
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                },
                DisplayedCountText = new LegacySpriteText(LegacyFont.Combo)
                {
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            };
        }

        /// <summary>
        /// Value shown at the current moment.
        /// </summary>
        public virtual int DisplayedCount
        {
            get => displayedCount;
            private set
            {
                if (displayedCount.Equals(value))
                    return;

                if (isRolling)
                    onDisplayedCountRolling(value);
                else if (displayedCount + 1 == value)
                    onDisplayedCountIncrement(value);
                else
                    onDisplayedCountChange(value);

                displayedCount = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DisplayedCountText.Text = FormatCount(Current.Value);
            PopOutCountText.Text = FormatCount(Current.Value);

            Current.BindValueChanged(combo => updateCount(combo.NewValue == 0), true);
        }

        private void updateCount(bool rolling)
        {
            int prev = previousValue;
            previousValue = Current.Value;

            if (!IsLoaded)
                return;

            if (!rolling)
            {
                FinishTransforms(false, nameof(DisplayedCount));

                isRolling = false;
                DisplayedCount = prev;

                if (prev + 1 == Current.Value)
                    IncrementCounter();
                else
                    SetCounter();
            }
            else
            {
                RollCounterToZero();
                isRolling = true;
            }
        }

        /// <summary>
        /// Increments the counter with transitions.
        /// </summary>
        protected virtual void IncrementCounter()
        {
            if (DisplayedCount < Current.Value - 1)
                DisplayedCount++;

            DisplayedCount++;
        }

        /// <summary>
        /// Rolls the counter back to zero.
        /// </summary>
        protected virtual void RollCounterToZero()
        {
            // Hides displayed count if was increasing from 0 to 1 but didn't finish
            if (DisplayedCount == 0)
                DisplayedCountText.FadeOut(fade_out_duration);

            transformRoll(DisplayedCount, 0);
        }

        /// <summary>
        /// Displays the new combo value on the counter with no transitions.
        /// </summary>
        protected virtual void SetCounter()
        {
            if (Current.Value == 0)
                DisplayedCountText.FadeOut();

            DisplayedCount = Current.Value;
        }

        private void onDisplayedCountRolling(int newValue)
        {
            if (newValue == 0)
                DisplayedCountText.FadeOut(fade_out_duration);

            DisplayedCountText.Text = FormatCount(newValue);
        }

        private void onDisplayedCountChange(int newValue)
        {
            DisplayedCountText.FadeTo(newValue == 0 ? 0 : 1);
            DisplayedCountText.Text = FormatCount(newValue);
        }

        private void onDisplayedCountIncrement(int newValue)
        {
            DisplayedCountText.Text = FormatCount(newValue);
        }

        private void transformRoll(int currentValue, int newValue) =>
            this.TransformTo(nameof(DisplayedCount), newValue, getProportionalDuration(currentValue, newValue));

        protected virtual string FormatCount(int count) => $@"{count}";

        private double getProportionalDuration(int currentValue, int newValue)
        {
            double difference = currentValue > newValue ? currentValue - newValue : newValue - currentValue;
            return difference * rolling_duration;
        }
    }
}
