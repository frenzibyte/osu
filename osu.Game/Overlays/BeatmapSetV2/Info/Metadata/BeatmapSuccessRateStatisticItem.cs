// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    public class BeatmapSuccessRateStatisticItem : BeatmapMetadataItem, IHasCurrentValue<APIBeatmap>
    {
        private readonly BindableWithCurrent<APIBeatmap> current = new BindableWithCurrent<APIBeatmap>();

        public Bindable<APIBeatmap> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly OsuSpriteText rate;
        private readonly Bar bar;

        public BeatmapSuccessRateStatisticItem()
            : base("Success Rate")
        {
            ItemFlow.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 2f),
                Children = new Drawable[]
                {
                    rate = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                    },
                    bar = new CircularBar
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 4f,
                        BackgroundColour = OverlayColourProvider.Red.Colour3,
                        AccentColour = OverlayColourProvider.Lime.Colour1,
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(b =>
            {
                int playCount = b.NewValue?.PlayCount ?? 0;
                int passCount = b.NewValue?.PassCount ?? 0;
                float successRate = playCount != 0 ? (float)passCount / playCount : 0;

                rate.Text = successRate.ToString("0.00%");
                bar.Length = successRate;
            }, true);
        }
    }
}
