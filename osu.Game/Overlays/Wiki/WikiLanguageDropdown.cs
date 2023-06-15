// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Wiki
{
    public partial class WikiLanguageDropdown : OsuDropdown<Language>
    {
        private readonly Language[] languages = Enum.GetValues<Language>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = 200;
            Items = languages;
        }

        protected override DropdownHeader CreateHeader() => new LanguageDropdownHeader { Current = Current };

        protected override DropdownMenu CreateMenu() => new LanguageDropdownMenu
        {
            MaxHeight = 300f
        };

        private partial class LanguageDropdownHeader : DropdownHeader, IHasCurrentValue<Language>
        {
            private readonly DrawableFlag flag;

            private readonly BindableWithCurrent<Language> current = new BindableWithCurrent<Language>();

            public Bindable<Language> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            protected override LocalisableString Label { get; set; }

            public LanguageDropdownHeader()
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;

                RelativeSizeAxes = Axes.None;
                AutoSizeAxes = Axes.Both;
                Margin = new MarginPadding { Bottom = 5f };

                Foreground.RelativeSizeAxes = Axes.None;
                Foreground.AutoSizeAxes = Axes.Both;
                Foreground.Padding = new MarginPadding(10f);
                Foreground.Child = flag = new DrawableFlag(CountryCode.Unknown)
                {
                    Scale = new Vector2(0.2f),
                    ShowTooltip = false,
                };

                Background.Masking = true;
                Background.CornerRadius = 20f;
                BackgroundColour = Color4.White.Opacity(0.2f);

                Background.Hide();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(v =>
                {
                    Foreground.Child = new DrawableFlag(v.NewValue.GetRepresentingCountry())
                    {
                        Scale = new Vector2(0.4f),
                        ShowTooltip = false,
                    };
                }, true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Background.FadeIn(200, Easing.OutQuint);
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeOut(200, Easing.OutQuint);
            }
        }

        private partial class LanguageDropdownMenu : OsuDropdownMenu
        {
            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new LanguageDrawableDropdownMenuItem(item)
            {
                BackgroundColourHover = HoverColour,
                BackgroundColourSelected = SelectionColour,
                Width = 0.95f,
            };

            private partial class LanguageDrawableDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                public LanguageDrawableDropdownMenuItem(MenuItem item)
                    : base(item)
                {
                }

                protected override Drawable CreateContent() => new LanguageContent(((DropdownMenuItem<Language>)Item).Value);

                private partial class LanguageContent : CompositeDrawable
                {
                    private readonly Language language;

                    public LanguageContent(Language language)
                    {
                        this.language = language;
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        InternalChild = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(5f),
                            Spacing = new Vector2(10f, 0f),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new DrawableFlag(language.GetRepresentingCountry())
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Scale = new Vector2(0.3f),
                                    Margin = new MarginPadding { Top = 10f },
                                    ShowTooltip = false,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = language.GetDescription(),
                                    Font = OsuFont.Default.With(size: 16),
                                }
                            }
                        };
                    }
                }
            }
        }
    }
}
