// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.IO.Archives;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public class TestSceneBeatmapSkinResources : OsuTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var imported = beatmaps.Import(new ZipArchiveReader(TestResources.OpenResource("Archives/ogg-beatmap.osz"))).Result;
            Beatmap.Value = beatmaps.GetWorkingBeatmap(imported.Beatmaps[0]);
        }

        [Test]
        public void TestRetrieveOggSample() => AddAssert("sample is non-null", () => Beatmap.Value.Skin.GetSample(new SampleInfo("sample")) != null);

        [Test]
        public void TestRetrieveOggTrack() => AddAssert("track is non-null", () => !MusicController.CurrentTrack.IsDummyDevice);
    }
}
