// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
    public class BeatmapUserRatingStatisticItem : BeatmapMetadataItem, IHasCurrentValue<APIBeatmapSet>
    {
        private readonly BindableWithCurrent<APIBeatmapSet> current = new BindableWithCurrent<APIBeatmapSet>();

        public Bindable<APIBeatmapSet> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly OsuSpriteText negativeRatings;
        private readonly OsuSpriteText positiveRatings;
        private readonly Bar bar;

        public BeatmapUserRatingStatisticItem()
            : base("User Rating")
        {
            ItemFlow.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 2f),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            negativeRatings = new OsuSpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                            },
                            positiveRatings = new OsuSpriteText
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold),
                            },
                        }
                    },
                    bar = new CircularBar
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 4f,
                        Direction = BarDirection.RightToLeft,
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
                int[] ratings = b.NewValue?.Ratings ?? Array.Empty<int>();

                int negative = ratings.Take(ratings.Length / 2).Sum();
                int positive = ratings.Skip(ratings.Length / 2).Sum();
                int total = negative + positive;

                negativeRatings.Text = negative.ToString();
                positiveRatings.Text = positive.ToString();
                bar.Length = total == 0 ? 0 : (float)positive / total;
            }, true);
        }
    }
}
