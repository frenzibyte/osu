// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    public abstract class BeatmapMetadataItem : CompositeDrawable
    {
        protected readonly FillFlowContainer ItemFlow;
        protected readonly Container MainContainer;
        protected readonly OsuSpriteText LabelText;

        protected BeatmapMetadataItem(string name)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = ItemFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Child = MainContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Child = LabelText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Text = name,
                        Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    },
                }
            };
        }
    }
}
