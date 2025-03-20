// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedge : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapContentWedgeHeader header = null!;
        private Container contentContainer = null!;

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public BeatmapContentWedge()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 10;
            Masking = true;

            const float header_height = 48f;

            InternalChild = new ShearAlignedFlowContainer(shear)
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    header = new BeatmapContentWedgeHeader
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = header_height,
                    },
                    contentContainer = new Container
                    {
                        // Padding = new MarginPadding { Top = header_height },
                        RelativeSizeAxes = Axes.Both,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            header.Type.BindValueChanged(_ => updateDisplay(), true);
        }

        private Drawable? currentContent;

        private void updateDisplay()
        {
            if (currentContent != null)
            {
                currentContent.MoveToX(-100f, 300, Easing.OutQuint);
                currentContent.FadeOut(300, Easing.OutQuint);
                currentContent.Expire();
            }

            switch (header.Type.Value)
            {
                default:
                case BeatmapContentWedgeHeader.ContentType.Details:
                    currentContent = new BeatmapContentWedgeDetails();
                    break;

                case BeatmapContentWedgeHeader.ContentType.Ranking:
                    currentContent = new BeatmapContentWedgeLeaderboard();
                    break;
            }

            contentContainer.Add(currentContent);
            currentContent.MoveToX(-100f).MoveToX(0f, 300, Easing.OutQuint);
            currentContent.FadeInFromZero(300, Easing.OutQuint);
        }
    }
}
