// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSetV2.Info;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapInfoOverlayDetails : OsuTestScene
    {
        private Bindable<BeatmapInfo> beatmap;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            beatmap = new Bindable<BeatmapInfo>(new TestBeatmap(Ruleset.Value).BeatmapInfo);

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(Bindable<BeatmapInfo>), beatmap),
                    (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Seafoam))
                },
                Child = new BeatmapInfoDetails
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        });
    }
}
