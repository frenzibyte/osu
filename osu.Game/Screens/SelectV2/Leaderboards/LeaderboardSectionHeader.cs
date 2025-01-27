// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;

namespace osu.Game.Screens.SelectV2.Leaderboards
{
    public partial class LeaderboardSectionHeader : CompositeDrawable
    {
        public readonly Bindable<BeatmapLeaderboardScope> Scope = new Bindable<BeatmapLeaderboardScope>();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            const float left_margin = 50f;

            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 10f;

            var shear = new Vector2(OsuGame.SHEAR, 0);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = left_margin, Vertical = 15f },
                    Shear = -shear,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 3),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Ranking", // todo: localisation
                            UseFullGlyphHeight = false,
                            Font = OsuFont.TorusAlternate.With(size: 18 * 1.2f, weight: FontWeight.SemiBold),
                        },
                        new Circle
                        {
                            RelativeSizeAxes = Axes.X,
                            Colour = colourProvider.Highlight1,
                            Height = 2f
                        },
                    },
                },
                new LeaderboardScopeControl
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 12 },
                    Shear = -shear,
                    Current = { BindTarget = Scope },
                },
            };
        }
    }
}
