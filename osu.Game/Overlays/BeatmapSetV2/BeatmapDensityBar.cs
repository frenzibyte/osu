// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSetV2
{
    public class BeatmapDensityBar : BeatmapMetricBar
    {
        [Resolved]
        private OsuColour colours { get; set; }

        protected override Color4 BackgroundColour => colours.BlueDark;

        protected override IEnumerable<(float startingNumber, Color4 colour)> Levels => Array.Empty<(float, Color4)>();

        // todo: pending web-side support.
        protected override IReadOnlyList<float> GetValuesFrom(APIBeatmap beatmap) => Array.Empty<float>();
    }
}
