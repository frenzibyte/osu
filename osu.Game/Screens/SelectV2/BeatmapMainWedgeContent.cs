// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMainWedgeContent : CompositeDrawable
    {
        public OsuSpriteText TitleLabel { get; private set; } = null!;
        public OsuSpriteText ArtistLabel { get; private set; } = null!;

        private readonly WorkingBeatmap working;
        private readonly IReadOnlyList<Mod> mods;

        private BeatmapMainWedgeStatistic playsStatistic = null!;
        private BeatmapMainWedgeStatistic favouritesStatistic = null!;

        private const float content_margin = SongSelect.WEDGE_CONTENT_MARGIN;

        public BeatmapMainWedgeContent(WorkingBeatmap working, IReadOnlyList<Mod> mods)
        {
            this.working = working;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // best effort to confine the auto-sized text to wedge bounds
            // the artist label doesn't have an extra text_margin as it doesn't touch the right metadata
            float shearWidth = OsuGame.SHEAR * DrawHeight;

            TitleLabel.MaxWidth = DrawWidth - content_margin * 2 - shearWidth;
            ArtistLabel.MaxWidth = DrawWidth - content_margin - shearWidth;
        }
    }
}
