// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Overlays.BeatmapSetV2;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneOnlineBeatmapInfoOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        private BeatmapInfoOverlay beatmapInfoOverlay;

        [SetUp]
        public void SetUp() => Schedule(() => Child = beatmapInfoOverlay = new BeatmapInfoOverlay());

        [Test]
        public void TestOnlineBeatmapSet()
        {
            AddStep("show 'my love'", () => beatmapInfoOverlay.FetchAndShowBeatmapSet(163112));
            AddStep("show 'soleily' mania", () => beatmapInfoOverlay.FetchAndShowBeatmap(557819));
        }
    }
}
