// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapRulesetTabControl : TabControl<IRulesetInfo>
    {
        [Resolved]
        private Bindable<IRulesetInfo> ruleset { get; set; }

        [Resolved]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            TabContainer.Padding = new MarginPadding { Top = 2f };
            TabContainer.Spacing = new Vector2(10f, 0f);

            SwitchTabOnRemove = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSet.BindValueChanged(set =>
            {
                Items = set.NewValue?.Beatmaps
                           .Select(b => b.Ruleset)
                           .Distinct()
                           .OrderBy(r => r.OnlineID)
                           .ToArray() ?? Array.Empty<IRulesetInfo>();
            }, true);

            Current = ruleset;
        }

        protected override Dropdown<IRulesetInfo> CreateDropdown() => null;

        protected override TabItem<IRulesetInfo> CreateTabItem(IRulesetInfo value) => new BeatmapRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Full,
        };

        private class BeatmapRulesetTabItem : TabItem<IRulesetInfo>
        {
            private RulesetIcon icon;

            public BeatmapRulesetTabItem(IRulesetInfo value)
                : base(value)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = icon = new RulesetIcon(Value)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20f),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
                FinishTransforms(true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e) => updateState();
            protected override void OnActivated() => updateState();
            protected override void OnDeactivated() => updateState();

            private void updateState()
            {
                if (Active.Value)
                    icon.FadeIn(200.0, Easing.OutQuint);
                else if (IsHovered)
                    icon.FadeTo(0.75f, 200.0, Easing.OutQuint);
                else
                    icon.FadeTo(0.25f, 200.0, Easing.OutQuint);
            }
        }
    }
}
