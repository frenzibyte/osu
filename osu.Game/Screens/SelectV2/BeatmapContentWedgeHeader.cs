// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedgeHeader : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapContentTabControl<ContentType> tabControl = null!;
        private FillFlowContainer leaderboardControls = null!;

        public IBindable<ContentType> Type => tabControl.Current;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = -shear,
                    Padding = new MarginPadding { Left = SongSelectV2.WEDGE_CONTENT_MARGIN, Right = 20f },
                    Children = new Drawable[]
                    {
                        tabControl = new BeatmapContentTabControl<ContentType>(24f)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Width = 200,
                            Height = 25,
                            Margin = new MarginPadding { Top = 5f },
                        },
                        leaderboardControls = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Size = new Vector2(128f, 30f),
                                    Child = new ShearedToggleButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Selected Mods",
                                        Height = 30f,
                                    },
                                },
                                new BeatmapContentTabControl<BeatmapLeaderboardScope>(24f)
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Width = 300,
                                    Height = 25,
                                    Margin = new MarginPadding { Top = 5f },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            tabControl.Current.BindValueChanged(v =>
            {
                leaderboardControls.FadeTo(v.NewValue == ContentType.Ranking ? 1 : 0, 300, Easing.OutQuint);
            }, true);
        }

        public enum ContentType
        {
            Details,
            Ranking,
        }
    }
}
