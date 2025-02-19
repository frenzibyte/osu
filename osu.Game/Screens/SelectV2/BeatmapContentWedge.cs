// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapContentWedge : CompositeDrawable
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private BeatmapContentWedgeHeader header = null!;
        private Container contentContainer = null!;

        public BeatmapContentWedge()
        {
            Width = 700;
            Height = 600;
            Y = 260;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Shear = shear;
            CornerRadius = 10;
            Masking = true;
            Margin = new MarginPadding { Left = -20 };

            InternalChildren = new Drawable[]
            {
                header = new BeatmapContentWedgeHeader
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 60,
                },
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 60 + 8 },
                },
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
