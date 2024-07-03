// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModCustomisationSection : CompositeDrawable
    {
        public readonly Mod Mod;

        private readonly IReadOnlyList<Drawable> settings;

        [Resolved]
        private ModCustomisationPanel.ModCustomisationScrollContainer scroll { get; set; } = null!;

        public ModCustomisationSection(Mod mod, IReadOnlyList<Drawable> settings)
        {
            Mod = mod;

            this.settings = settings;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            FillFlowContainer flow;

            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 8f),
                Padding = new MarginPadding { Left = 7f },
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 20f, Right = 27f },
                        Margin = new MarginPadding { Bottom = 4f },
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = Mod.Name,
                                Font = OsuFont.TorusAlternate.With(size: 20, weight: FontWeight.SemiBold),
                            },
                            new ModSwitchTiny(Mod)
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Active = { Value = true },
                                Scale = new Vector2(0.5f),
                            }
                        }
                    },
                }
            };

            foreach (var dropdown in settings.Cast<ISettingsItem>().OfType<ISettingsDropdown>())
                dropdown.MenuStateChanged += _ => menuStateChanged(dropdown);

            flow.AddRange(settings);
        }

        private ISettingsDropdown? openDropdown;

        private void menuStateChanged(ISettingsDropdown dropdown)
        {
            if (dropdown.State == MenuState.Closed)
            {
                if (dropdown == openDropdown)
                    openDropdown = null;

                return;
            }

            openDropdown = dropdown;
            scrollDropdownIntoView(dropdown);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FinishTransforms(true);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (openDropdown != null && !scroll.UserScrolling)
                scrollDropdownIntoView(openDropdown);
        }

        private void scrollDropdownIntoView(ISettingsDropdown dropdown) => scroll.ScrollIntoView((Drawable)dropdown, extraScroll: 30);
    }
}
