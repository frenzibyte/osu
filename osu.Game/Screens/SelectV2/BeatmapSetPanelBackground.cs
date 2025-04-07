// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanelBackground : ModelBackedDrawable<WorkingBeatmap>
    {
        public readonly Bindable<float> AverageHue = new Bindable<float>();

        protected override double TransformDuration => 400;

        public WorkingBeatmap? Beatmap
        {
            get => Model;
            set => Model = value;
        }

        protected override Drawable CreateDrawable(WorkingBeatmap? model) => new BackgroundSprite(model)
        {
            AverageHue = { BindTarget = AverageHue },
        };

        private partial class BackgroundSprite : CompositeDrawable
        {
            private readonly WorkingBeatmap? working;

            public readonly Bindable<float> AverageHue = new Bindable<float>();

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
                    float hue = texture.Average.ToHSL().X;
                    AverageHue.Value = hue;

                    Colour4 colour = Colour4.FromHSL(hue, 0.3f, 0.3f);

                    InternalChildren = new Drawable[]
                    {
                        new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                            Texture = texture,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(colour, colour.Opacity(0.25f)),
                        },
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
