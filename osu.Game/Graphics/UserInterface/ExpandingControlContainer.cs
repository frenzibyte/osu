// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ExpandingControlContainer : CompositeDrawable
    {
        public Bindable<bool> ForceExpanded { get; } = new BindableBool();

        public LocalisableString CollapsedText
        {
            get => collapsedSpriteText.Text;
            set => collapsedSpriteText.Text = value;
        }

        public Drawable ExpandedContent
        {
            get => expandedContentContainer.Child;
            set => expandedContentContainer.Child = value;
        }

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        private readonly OsuSpriteText collapsedSpriteText;
        private readonly Container expandedContentContainer;

        public ExpandingControlContainer()
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                collapsedSpriteText = new OsuSpriteText(),
                expandedContentContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Alpha = 0
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ForceExpanded.BindValueChanged(_ => updateState(), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            bool expanded = IsHovered || ForceExpanded.Value;

            collapsedSpriteText.Alpha = expanded ? 0 : 1;
            expandedContentContainer.Alpha = expanded ? 1 : 0;
        }
    }
}
