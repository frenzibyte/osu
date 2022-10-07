// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyCatchComboCounter : LegacyRulesetComboCounter
    {
        private Catcher catcher = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        protected override bool ShouldBeAlive => drawableRuleset is DrawableCatchRuleset;

        public LegacyCatchComboCounter()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.Centre;

            Position = new Vector2(CatchPlayfield.CENTER_X, -250);
            Scale = new Vector2(0.8f);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (drawableRuleset is not DrawableCatchRuleset drawableCatchRuleset)
                return;

            catcher = drawableCatchRuleset.Playfield.Catcher;
        }

        private IBindable<JudgementResult> lastJudgementResult = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            lastJudgementResult = gameplayState.LastJudgementResult.GetBoundCopy();
            lastJudgementResult.BindValueChanged(r =>
            {
                IHasComboInformation? catchHitObject = (IHasComboInformation?)r.NewValue?.HitObject;
                PopOutCountText.Colour = catchHitObject?.GetComboColour(skin) ?? Color4.White;
            }, true);
        }

        protected override void Update()
        {
            base.Update();
            X = catcher.X;
        }

        protected override void IncrementCounter()
        {
            this.FadeInFromZero().Then().Delay(1000).FadeOut(300);

            DisplayedCountText.ScaleTo(1.5f)
                              .ScaleTo(0.8f, 250, Easing.Out)
                              .OnComplete(c => base.IncrementCounter());

            DisplayedCountText.Delay(250)
                              .ScaleTo(1f)
                              .ScaleTo(1.1f, 60).Then().ScaleTo(1f, 30);

            PopOutCountText.Text = FormatCount(Current.Value);
            PopOutCountText.ScaleTo(1.5f)
                           .ScaleTo(1.9f, 400, Easing.Out)
                           .FadeOutFromOne(400);
        }

        protected override void SetCounter()
        {
            base.SetCounter();
            Hide();
        }

        protected override void RollCounterToZero()
        {
            base.RollCounterToZero();
            this.FadeOut(400, Easing.Out);
        }
    }

    /// <summary>
    /// A combo counter implementation that visually behaves almost similar to stable's osu!catch combo counter.
    /// </summary>
    public class LegacyCatchComboCounterOld : CompositeDrawable, ICatchComboCounter
    {
        private readonly LegacyRollingCounter counter;

        private readonly LegacyRollingCounter explosion;

        public LegacyCatchComboCounterOld()
        {
            AutoSizeAxes = Axes.Both;

            Alpha = 0f;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);

            InternalChildren = new Drawable[]
            {
                explosion = new LegacyRollingCounter(LegacyFont.Combo)
                {
                    Alpha = 0.65f,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                },
                counter = new LegacyRollingCounter(LegacyFont.Combo)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        private int lastDisplayedCombo;

        public void UpdateCombo(int combo, Color4? hitObjectColour = null)
        {
            if (combo == lastDisplayedCombo)
                return;

            // There may still be existing transforms to the counter (including value change after 250ms),
            // finish them immediately before new transforms.
            counter.SetCountWithoutRolling(lastDisplayedCombo);

            lastDisplayedCombo = combo;

            if (Time.Elapsed < 0)
            {
                // needs more work to make rewind somehow look good.
                // basically we want the previous increment to play... or turning off RemoveCompletedTransforms (not feasible from a performance angle).
                Hide();
                return;
            }

            // Combo fell to zero, roll down and fade out the counter.
            if (combo == 0)
            {
                counter.Current.Value = 0;
                explosion.Current.Value = 0;
            }
            else
            {
                this.FadeInFromZero().Then().Delay(1000).FadeOut(300);

                counter.ScaleTo(1.5f)
                       .ScaleTo(0.8f, 250, Easing.Out)
                       .OnComplete(c => c.SetCountWithoutRolling(combo));

                counter.Delay(250)
                       .ScaleTo(1f)
                       .ScaleTo(1.1f, 60).Then().ScaleTo(1f, 30);

                explosion.Colour = hitObjectColour ?? Color4.White;

                explosion.SetCountWithoutRolling(combo);
                explosion.ScaleTo(1.5f)
                         .ScaleTo(1.9f, 400, Easing.Out)
                         .FadeOutFromOne(400);
            }
        }
    }
}
