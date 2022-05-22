// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osuTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasPosition
    {
        /// <summary>
        /// The radius of hit objects (ie. the radius of a <see cref="HitCircle"/>).
        /// </summary>
        public const float OBJECT_RADIUS = 64;

        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second (ie. the speed slider balls move through their track).
        /// </summary>
        internal const float BASE_SCORING_DISTANCE = 100;

        /// <summary>
        /// Minimum preempt time at AR=10.
        /// </summary>
        public const double PREEMPT_MIN = 450;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        private IndirectBindable<Vector2> positionBindable;

        public Bindable<Vector2> PositionBindable => positionBindable ??= new IndirectBindable<Vector2>(() => Position, v => Position = v);

        public virtual Vector2 Position { get; set; }

        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        private IndirectBindable<int> stackHeightBindable;

        public Bindable<int> StackHeightBindable => stackHeightBindable ??= new IndirectBindable<int>(() => StackHeight, v => StackHeight = v);

        public virtual int StackHeight { get; set; }

        public virtual Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        public double Radius => OBJECT_RADIUS * Scale;

        private IndirectBindable<float> scaleBindable;

        public Bindable<float> ScaleBindable => scaleBindable ??= new IndirectBindable<float>(() => Scale, v => Scale = v);

        public virtual float Scale { get; set; }

        public virtual bool NewCombo { get; set; }

        private IndirectBindable<int> comboOffsetBindable;

        public Bindable<int> ComboOffsetBindable => comboOffsetBindable ??= new IndirectBindable<int>(() => ComboOffset, v => ComboOffset = v);

        public virtual int ComboOffset { get; set; }

        private IndirectBindable<int> indexInCurrentComboBindable;

        public Bindable<int> IndexInCurrentComboBindable => indexInCurrentComboBindable ??= new IndirectBindable<int>(() => IndexInCurrentCombo, v => IndexInCurrentCombo = v);

        public virtual int IndexInCurrentCombo { get; set; }

        private IndirectBindable<int> comboIndexBindable;

        public Bindable<int> ComboIndexBindable => comboIndexBindable ??= new IndirectBindable<int>(() => ComboIndex, v => ComboIndex = v);

        public virtual int ComboIndex { get; set; }

        private IndirectBindable<int> comboIndexWithOffsetsBindable;

        public Bindable<int> ComboIndexWithOffsetsBindable => comboIndexWithOffsetsBindable ??= new IndirectBindable<int>(() => ComboIndexWithOffsets, v => ComboIndexWithOffsets = v);

        public virtual int ComboIndexWithOffsets { get; set; }

        private IndirectBindable<bool> lastInComboBindable;

        public Bindable<bool> LastInComboBindable => lastInComboBindable ??= new IndirectBindable<bool>(() => LastInCombo, v => LastInCombo = v);

        public virtual bool LastInCombo { get; set; }

        protected OsuHitObject()
        {
            StackHeightBindable.BindValueChanged(height =>
            {
                foreach (var nested in NestedHitObjects.OfType<OsuHitObject>())
                    nested.StackHeight = height.NewValue;
            });
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 1800, 1200, PREEMPT_MIN);

            // Preempt time can go below 450ms. Normally, this is achieved via the DT mod which uniformly speeds up all animations game wide regardless of AR.
            // This uniform speedup is hard to match 1:1, however we can at least make AR>10 (via mods) feel good by extending the upper linear function above.
            // Note that this doesn't exactly match the AR>10 visuals as they're classically known, but it feels good.
            // This adjustment is necessary for AR>10, otherwise TimePreempt can become smaller leading to hitcircles not fully fading in.
            TimeFadeIn = 400 * Math.Min(1, TimePreempt / PREEMPT_MIN);

            Scale = (1.0f - 0.7f * (difficulty.CircleSize - 5) / 5) / 2;
        }

        protected override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
