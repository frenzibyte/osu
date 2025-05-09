// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        [Test]
        public void TestArtistGrouping()
        {
            AddStep("add beatmaps", () =>
            {
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "Fast"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "Ace"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "-test"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "@test"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "fest"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "ace"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "5ce"));
                BeatmapSets.Add(createBeatmapWith(m => m.Artist = "4ce"));
            });

            GroupBy(GroupMode.Artist);
            WaitForFiltering();

            AddAssert("group 0 is 0-9");
        }

        private void groupNameAt(int index, string name)
        {
            AddAssert($"group {index} name = {name}",
                () => Grouping.AllItems.Select(i => i.Model).OfType<GroupDefinition>().ElementAt(index).Title,
                () => Is.EqualTo(name));
        }

        private void groupContains(int index, IEnumerable<BeatmapInfo> beatmaps)
        {
            AddAssert("group ");
        }

        private BeatmapSetInfo createBeatmapWith(Action<BeatmapMetadata> applyToMetadata)
        {
            var set = TestResources.CreateTestBeatmapSetInfo();
            applyToMetadata(set.Beatmaps[0].Metadata);
            return set;
        }
    }
}
