// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetOnlineExplicitPill : CircularContainer
    {
        private readonly Box background;
        private readonly OsuSpriteText label;

        public float BackgroundAlpha
        {
            get => background.Alpha;
            set => background.Alpha = value;
        }

        public float TextSize
        {
            get => label.Font.Size;
            set => label.Font = label.Font.With(size: value);
        }

        public MarginPadding TextPadding
        {
            get => label.Padding;
            set => label.Padding = value;
        }

        public BeatmapSetOnlineExplicitPill()
        {
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                label = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                    Text = BeatmapsetsStrings.NsfwBadgeLabel.ToUpper(),
                },
            };

            TextPadding = new MarginPadding { Horizontal = 5f, Bottom = 1f };
        }
    }
}
