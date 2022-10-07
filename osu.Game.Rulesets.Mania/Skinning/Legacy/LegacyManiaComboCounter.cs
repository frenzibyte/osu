// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class LegacyManiaComboCounter : LegacyRulesetComboCounter
    {
        [Resolved]
        private DrawableRuleset? drawableRuleset { get; set; }

        protected override bool ShouldBeAlive => drawableRuleset is DrawableManiaRuleset;

        public LegacyManiaComboCounter()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Y = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ComboPosition)?.Value ?? 0;

            DisplayedCountText.Anchor = Anchor.Centre;
            DisplayedCountText.Origin = Anchor.Centre;

            PopOutCountText.Anchor = Anchor.Centre;
            PopOutCountText.Origin = Anchor.Centre;
            PopOutCountText.Colour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ComboBreakColour)?.Value ?? Color4.Red;
        }

        protected override void IncrementCounter()
        {
            base.IncrementCounter();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(new Vector2(1f, 1.4f))
                              .ScaleTo(new Vector2(1f), 300, Easing.Out)
                              .FadeIn(120);
        }

        protected override void SetCounter()
        {
            base.SetCounter();

            PopOutCountText.Hide();
            DisplayedCountText.ScaleTo(1f);
        }

        protected override void RollCounterToZero()
        {
            if (DisplayedCount > 0)
            {
                PopOutCountText.Text = FormatCount(DisplayedCount);
                PopOutCountText.FadeTo(0.8f).FadeOut(200)
                               .ScaleTo(1f).ScaleTo(4f, 200);

                DisplayedCountText.FadeTo(0.5f, 300);
            }

            base.RollCounterToZero();
        }
    }
}
