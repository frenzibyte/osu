// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.BeatmapSetV2.Header;

namespace osu.Game.Overlays.BeatmapSetV2
{
    public class BeatmapInfoHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new BeatmapInfoTitle();

        protected override Drawable CreateTitleContent() => new BeatmapRulesetTabControl();

        private class BeatmapInfoTitle : OverlayTitle
        {
            public BeatmapInfoTitle()
            {
                Title = "Beatmap Info";
                IconTexture = @"Icons/Hexacons/beatmap";
            }
        }
    }
}
