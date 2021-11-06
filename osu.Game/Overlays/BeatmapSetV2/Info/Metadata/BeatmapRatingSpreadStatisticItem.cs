// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    public class BeatmapRatingSpreadStatisticItem : BeatmapMetadataItem, IHasCurrentValue<APIBeatmapSet>
    {
        private readonly BindableWithCurrent<APIBeatmapSet> current = new BindableWithCurrent<APIBeatmapSet>();

        public Bindable<APIBeatmapSet> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BarGraph graph;

        public BeatmapRatingSpreadStatisticItem()
            : base("Rating Spread")
        {
            ItemFlow.Add(graph = new BarGraph(bar =>
            {
                bar.bar.CornerRadius = 2f;

                // todo: needs explaination, feels odd at first glance.
                bar.bar.MinLength = -0.05f;

                // todo: the original design applies a linear gradient over the bar graph, but that's not supported in osu!framework currently.
                // for now, each bar receives a colour roughly similar to the one in the design.
                bar.bar.AccentColour = Color4.FromHsl(new Vector4(MathF.Min(bar.index * 0.25f / (bar.count - 2), 0.25f), 1f, 0.7f, 1f));
            })
            {
                RelativeSizeAxes = Axes.X,
                Height = 35f,
                Direction = BarDirection.BottomToTop,
                Spacing = new Vector2(2f, 0f),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(b =>
            {
                graph.Values = b.NewValue?.Ratings.Skip(1).Select(r => (float)r) ?? Enumerable.Repeat(0f, 10);
            }, true);
        }
    }
}
