// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2.Leaderboards
{
    public partial class LeaderboardScopeControl : TabControl<BeatmapLeaderboardScope>
    {
        protected override Dropdown<BeatmapLeaderboardScope>? CreateDropdown() => null;

        protected override TabItem<BeatmapLeaderboardScope> CreateTabItem(BeatmapLeaderboardScope value) => new ScopeTabItem(value);

        public LeaderboardScopeControl()
        {
            // Add in reverse because the tab control is anchored from the right, and we want the first scope to be displayed leftmost.
            foreach (var item in Enum.GetValues<BeatmapLeaderboardScope>().Reverse())
                AddItem(item);

            Current.Value = BeatmapLeaderboardScope.Local;
        }

        private partial class ScopeTabItem : TabItem<BeatmapLeaderboardScope>
        {
            private Container background = null!;
            private OsuSpriteText text = null!;

            private Sample selectSample = null!;

            public ScopeTabItem(BeatmapLeaderboardScope value)
                : base(value)
            {
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                Anchor = Anchor.CentreRight;
                Origin = Anchor.CentreRight;

                Size = new Vector2(80, 25);

                Vector2 shear = new Vector2(OsuGame.SHEAR, 0f);
                float shearWidth = shear.X * Height;

                Margin = new MarginPadding { Left = shearWidth * 2 };

                Children = new Drawable[]
                {
                    background = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Shear = shear,
                        CornerRadius = 5f,
                        Child = new Box { RelativeSizeAxes = Axes.Both },
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = OsuColour.ForegroundTextColourFor(colourForScope(Value)),
                        UseFullGlyphHeight = true,
                        Y = -1f,
                        Margin = new MarginPadding { Right = shearWidth / 2 },
                        Text = textForScope(Value),
                    },
                    new HoverSounds(HoverSampleSet.TabSelect),
                };

                selectSample = audio.Samples.Get(@"UI/tabselect-select");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateDisplay();
                FinishTransforms(true);
            }

            protected override void OnActivated() => updateDisplay();

            protected override void OnDeactivated() => updateDisplay();

            protected override void OnActivatedByUser() => selectSample.Play();

            protected override bool OnHover(HoverEvent e)
            {
                updateDisplay();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateDisplay();
                base.OnHoverLost(e);
            }

            private void updateDisplay()
            {
                Color4 colour;

                if (Active.Value)
                    colour = colourForScope(Value);
                else if (IsHovered)
                    colour = colourForScope(Value).Darken(0.5f);
                else
                    colour = colourProvider.Dark2;

                text.Font = text.Font.With(weight: Active.Value ? FontWeight.SemiBold : FontWeight.Regular);
                text.Colour = OsuColour.ForegroundTextColourFor(colour);

                background.FadeColour(colour, 200, Easing.OutQuint);
            }

            private static Color4 colourForScope(BeatmapLeaderboardScope scope)
            {
                switch (scope)
                {
                    case BeatmapLeaderboardScope.Local:
                        return Color4Extensions.FromHex("FF5A6D");

                    case BeatmapLeaderboardScope.Global:
                        return Color4Extensions.FromHex("FFA95A");

                    case BeatmapLeaderboardScope.Country:
                        return Color4Extensions.FromHex("CAFF5A");

                    case BeatmapLeaderboardScope.Friend:
                        return Color4Extensions.FromHex("5AFFF5");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private static string textForScope(BeatmapLeaderboardScope scope)
            {
                switch (scope)
                {
                    case BeatmapLeaderboardScope.Local:
                        return "Local";

                    case BeatmapLeaderboardScope.Global:
                        return "Global";

                    case BeatmapLeaderboardScope.Country:
                        return "Country";

                    case BeatmapLeaderboardScope.Friend:
                        return "Friends";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
