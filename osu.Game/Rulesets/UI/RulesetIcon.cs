// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.UI
{
    public class RulesetIcon : CompositeDrawable, IHasCurrentValue<IRulesetInfo>
    {
        private readonly BindableWithCurrent<IRulesetInfo> current = new BindableWithCurrent<IRulesetInfo>();

        public Bindable<IRulesetInfo> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private ConstrainedIconContainer iconContainer;

        public RulesetIcon(IRulesetInfo ruleset = null)
        {
            Current.Value = ruleset;
        }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = iconContainer = new ConstrainedIconContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(r =>
            {
                var localRuleset = Current.Value == null ? null : rulesets.GetRuleset(Current.Value.OnlineID);

                var icon = localRuleset?.CreateInstance()?.CreateIcon();
                icon ??= new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };
                iconContainer.Icon = icon;
            }, true);
        }
    }
}
