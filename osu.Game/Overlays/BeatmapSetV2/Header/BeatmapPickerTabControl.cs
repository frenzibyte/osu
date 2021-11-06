// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapPickerTabControl : TabControl<APIBeatmap>
    {
        private const float icon_spacing = 5f;

        private readonly FillFlowContainer strips;

        [Resolved]
        private IBindable<IReadOnlyList<APIBeatmap>> availableBeatmaps { get; set; }

        public BeatmapPickerTabControl()
        {
            AutoSizeAxes = Axes.Both;
            TabContainer.Spacing = new Vector2(icon_spacing, 0f);

            SwitchTabOnRemove = false;

            AddInternal(strips = new FillFlowContainer
            {
                Depth = 1f,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(icon_spacing, 0f),
                AutoSizeAxes = Axes.Both,
                Y = BeatmapPicker.PICKER_STRIP_SPACING,
            });
        }

        [Resolved]
        private OsuColour colours { get; set; }

        protected override void LoadComplete()
        {
            availableBeatmaps.BindValueChanged(beatmaps =>
            {
                strips.Clear();

                Items = beatmaps.NewValue;

                foreach (var beatmapsPerRuleset in beatmaps.NewValue.GroupBy(i => i.Ruleset))
                {
                    foreach (var beatmapsPerRating in beatmapsPerRuleset.GroupBy(i => BeatmapDifficultyCache.GetDifficultyRating(i.StarRating)))
                    {
                        strips.Add(new Box
                        {
                            Colour = colours.ForDifficultyRating(beatmapsPerRating.Key),
                            Width = (BeatmapPickerTabItem.ICON_SIZE + icon_spacing) * (beatmapsPerRating.Count() - 1) + BeatmapPickerTabItem.ICON_SIZE,
                            Height = 1f,
                        });
                    }
                }
            }, true);

            base.LoadComplete();
        }

        protected override Dropdown<APIBeatmap> CreateDropdown() => null;

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Full,
        };

        protected override TabItem<APIBeatmap> CreateTabItem(APIBeatmap value) => new BeatmapPickerTabItem(value);

        private class BeatmapPickerTabItem : TabItem<APIBeatmap>
        {
            public const float ICON_SIZE = 20f;

            private ExpandingBar bar;

            public BeatmapPickerTabItem(APIBeatmap value)
                : base(value)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    new RulesetIcon(Value.Ruleset)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(ICON_SIZE),
                        Colour = colours.ForStarDifficulty(Value.StarRating),
                    },
                    bar = new ExpandingBar
                    {
                        Anchor = Anchor.BottomCentre,
                        Colour = colours.ForDifficultyRating(BeatmapDifficultyCache.GetDifficultyRating(Value.StarRating)),
                        ExpandedSize = 5f,
                        CollapsedSize = 0f,
                        Expanded = false,
                        Y = BeatmapPicker.PICKER_STRIP_SPACING,
                    },
                    new HoverClickSounds(HoverSampleSet.TabSelect),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e) => updateState();
            protected override void OnActivated() => updateState();
            protected override void OnDeactivated() => updateState();

            private void updateState()
            {
                if (Active.Value || IsHovered)
                    bar.Expand();
                else
                    bar.Collapse();
            }
        }
    }
}
