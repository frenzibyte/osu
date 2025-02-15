// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class BeatmapBackgroundSprite : Sprite
    {
        private readonly IWorkingBeatmap working;

        public BeatmapBackgroundSprite(IWorkingBeatmap working)
        {
            ArgumentNullException.ThrowIfNull(working);

            this.working = working;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            var background = working.GetBackground();
            if (background != null)
                Texture = background;

            Texture = textures.Get("https://media.discordapp.net/attachments/418500862319525892/1340379678346711172/Rectangle_4899.png?ex=67b2255a&is=67b0d3da&hm=f5a047825befe710e94942067d835d3fb31ce6e5af725bbcb97af798a9589cd5&=&format=webp&quality=lossless&width=1126&height=907");
        }
    }
}
