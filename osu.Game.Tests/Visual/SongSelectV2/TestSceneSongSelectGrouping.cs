// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Collections;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectGrouping : SongSelectTestScene
    {
        private BeatmapCarouselFilterGrouping grouping => Carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single();

        [Test]
        public void TestCollectionGrouping()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            AddStep("add collections", () =>
            {
                var beatmaps = Beatmaps.GetAllUsableBeatmapSets().OrderBy(b => b.OnlineID).ToArray();

                Realm.Write(r =>
                {
                    r.Add(new BeatmapCollection("My Collection #1", beatmaps[0].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #2", beatmaps[1].Beatmaps.Select(b => b.MD5Hash).ToList()));
                    r.Add(new BeatmapCollection("My Collection #3"));
                });
            });

            LoadSongSelect();
            GroupBy(GroupMode.Collections);

            AddUntilStep("wait for filtering", () => !Carousel.IsFiltering);
        }
    }
}
