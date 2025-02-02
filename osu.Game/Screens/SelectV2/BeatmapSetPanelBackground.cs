// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanelBackground : ModelBackedDrawable<WorkingBeatmap>
    {
        protected override bool TransformImmediately => true;

        public WorkingBeatmap? Beatmap
        {
            get => Model;
            set => Model = value;
        }

        protected override Drawable CreateDrawable(WorkingBeatmap? model) => new BackgroundSprite(model);

        private partial class BackgroundSprite : CompositeDrawable
        {
            private readonly WorkingBeatmap? working;

            public BackgroundSprite(WorkingBeatmap? working)
            {
                this.working = working;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                var texture = working?.GetPanelBackground();

                if (texture != null)
                {
                    InternalChild = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fill,
                        Texture = texture,
                    };
                }
                else
                {
                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background6,
                    };
                }
            }
        }
    }
}
