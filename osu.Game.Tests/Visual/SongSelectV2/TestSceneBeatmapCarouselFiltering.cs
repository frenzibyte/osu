// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselFiltering : BeatmapCarouselTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();
        }

        [Test]
        public void TestBasicFiltering()
        {
            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            SelectNextPanel();
            Select();

            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);
            WaitForFiltering();

            CheckVisibleBeatmapsCount(3);
            CheckVisibleBeatmapSetsCount(1);
            WaitForSelection(2, 0);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            WaitForSelection(2, 1);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForFiltering();

            CheckVisibleBeatmapsCount(30);
            CheckVisibleBeatmapSetsCount(10);
        }

        [Test]
        public void TestFilteringByUserStarDifficulty()
        {
            AddStep("add mixed difficulty set", () =>
            {
                var set = TestResources.CreateTestBeatmapSetInfo(1);
                set.Beatmaps.Clear();

                for (int i = 1; i <= 15; i++)
                {
                    set.Beatmaps.Add(new BeatmapInfo(new OsuRuleset().RulesetInfo, new BeatmapDifficulty(), new BeatmapMetadata())
                    {
                        BeatmapSet = set,
                        DifficultyName = $"Stars: {i}",
                        StarRating = i,
                    });
                }

                BeatmapSets.Add(set);
            });

            WaitForDrawablePanels();

            ApplyToFilter("filter [5..]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = null;
            });
            WaitForFiltering();
            CheckVisibleBeatmapsCount(11);

            ApplyToFilter("filter to [0..7]", c =>
            {
                c.UserStarDifficulty.Min = null;
                c.UserStarDifficulty.Max = 7;
            });
            WaitForFiltering();
            CheckVisibleBeatmapsCount(7);

            ApplyToFilter("filter to [5..7]", c =>
            {
                c.UserStarDifficulty.Min = 5;
                c.UserStarDifficulty.Max = 7;
            });

            WaitForFiltering();
            CheckVisibleBeatmapsCount(3);

            ApplyToFilter("filter to [2..2]", c =>
            {
                c.UserStarDifficulty.Min = 2;
                c.UserStarDifficulty.Max = 2;
            });

            WaitForFiltering();
            CheckVisibleBeatmapsCount(1);

            ApplyToFilter("filter to [0..]", c =>
            {
                c.UserStarDifficulty.Min = 0;
                c.UserStarDifficulty.Max = null;
            });
            WaitForFiltering();
            CheckVisibleBeatmapsCount(15);
        }

        [Test]
        public void TestSelectionChangesWithFiltering()
        {
            AddBeatmaps(10, 3);
            SelectNextPanel();

            ApplyToFilter("filter some difficulties", c => c.SearchText = "Normal");
            WaitForSelection(0, 0);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForSelection(0, 0);

            ApplyToFilter("filter all", c => c.SearchText = "Dingo");

            CheckVisibleBeatmapsCount(0);
            AddAssert("no sets displayed", () => Carousel.BeatmapSetsCount == 0);
            AddAssert("selection is null", () => Carousel.CurrentSelection == null);

            SelectNextPanel();
            AddAssert("selection is null", () => Carousel.CurrentSelection == null);

            SelectNextGroup();
            AddAssert("selection is null", () => Carousel.CurrentSelection == null);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);

            AddUntilStep("selection is not null", () => Carousel.CurrentSelection != null);
        }

        [Test]
        public void TestFilterRange()
        {
            string searchText = null!;

            AddBeatmaps(10, 3);
            WaitForDrawablePanels();

            // buffer the selection
            SelectNextGroup();
            SelectNextGroup();
            SelectNextPanel();
            Select();

            AddStep("get search text", () => searchText = ((BeatmapInfo)Carousel.CurrentSelection!).Metadata.Title);

            SelectPrevGroup();
            SelectPrevGroup();
            SelectNextPanel();
            SelectNextPanel();
            Select();

            ApplyToFilter("apply a range filter", c =>
            {
                c.SearchText = searchText;
                c.StarDifficulty = new FilterCriteria.OptionalRange<double>
                {
                    Min = 2,
                    Max = 5.5,
                    IsLowerInclusive = true
                };
            });

            // should reselect the buffered selection.
            WaitForSelection(2, 1);
        }

        [Test]
        public void TestExternalRulesetChange()
        {
            ApplyToFilter("allow converted beatmaps", c => c.AllowConvertedBeatmaps = true);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));

            WaitForFiltering();

            AddStep("add mixed ruleset beatmapset", () =>
            {
                var testMixed = TestResources.CreateTestBeatmapSetInfo(3);

                for (int i = 0; i <= 2; i++)
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);

                BeatmapSets.Add(testMixed);
            });
            WaitForDrawablePanels();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 1
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1;
            });

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));

            WaitForFiltering();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 1) == 1;
            });

            ApplyToFilter("filter to catch", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(2));

            WaitForFiltering();

            AddUntilStep("wait for filtered difficulties", () =>
            {
                var visibleBeatmapPanels = GetVisiblePanels<PanelBeatmap>();

                return visibleBeatmapPanels.Count() == 2
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 0) == 1
                       && visibleBeatmapPanels.Count(p => ((BeatmapInfo)p.Item!.Model).Ruleset.OnlineID == 2) == 1;
            });
        }

        [Test]
        [Ignore("Difficulty sorting behaves unexpectedly with this test. Not sure if we will continue to use it.")]
        // todo: fix this.
        public void TestSortingWithDifficultyFiltered()
        {
            const int diffs_per_set = 3;
            const int local_set_count = 2;

            AddStep("populate beatmap sets", () =>
            {
                for (int i = 0; i < local_set_count; i++)
                {
                    var set = TestResources.CreateTestBeatmapSetInfo(diffs_per_set);
                    set.Beatmaps[0].StarRating = 3 - i;
                    set.Beatmaps[0].DifficultyName += $" ({3 - i}*)";
                    set.Beatmaps[1].StarRating = 6 + i;
                    set.Beatmaps[1].DifficultyName += $" ({6 + i}*)";
                    BeatmapSets.Add(set);
                }
            });

            SortBy(SortMode.Difficulty);

            CheckVisibleBeatmapSetsCount(local_set_count * diffs_per_set);
            CheckVisibleBeatmapsCount(local_set_count * diffs_per_set);

            ApplyToFilter("filter to normal", c => c.SearchText = "Normal");

            CheckVisibleBeatmapSetsCount(local_set_count);
            CheckVisibleBeatmapsCount(local_set_count);

            ApplyToFilter("filter to insane", c => c.SearchText = "Insane");

            CheckVisibleBeatmapSetsCount(local_set_count);
            CheckVisibleBeatmapsCount(local_set_count);
        }

        [Test]
        public void TestSelectionEnteringFromEmptyRuleset()
        {
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddStep("create beatmaps for taiko only", () =>
            {
                var rulesetBeatmapSet = TestResources.CreateTestBeatmapSetInfo(1);
                var taikoRuleset = rulesets.AvailableRulesets.ElementAt(1);
                rulesetBeatmapSet.Beatmaps.ForEach(b => b.Ruleset = taikoRuleset);

                BeatmapSets.Add(rulesetBeatmapSet);
            });

            AddAssert("selection is null", () => Carousel.CurrentSelection == null);

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("selection is not null", () => Carousel.CurrentSelection != null);
        }

        [Test]
        public void TestSelectingFilteredRuleset()
        {
            AddStep("add mixed ruleset beatmapset", () =>
            {
                var testMixed = TestResources.CreateTestBeatmapSetInfo(3);

                for (int i = 0; i <= 2; i++)
                    testMixed.Beatmaps[i].Ruleset = rulesets.AvailableRulesets.ElementAt(i);

                BeatmapSets.Add(testMixed);
            });

            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddUntilStep("osu difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 0);

            RemoveAllBeatmaps();

            AddStep("add single ruleset beatmapset", () =>
            {
                var testSingle = TestResources.CreateTestBeatmapSetInfo(3);
                testSingle.Beatmaps.ForEach(b => b.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
                BeatmapSets.Add(testSingle);
            });
            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
            ApplyToFilter("filter to osu", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(0));
            AddUntilStep("no difficulty selected", () => Carousel.CurrentSelection == null);
            ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
            AddUntilStep("taiko difficulty selected", () => ((BeatmapInfo?)Carousel.CurrentSelection)?.Ruleset.OnlineID == 1);
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            HashSet<Guid> eagerSelectedIDs = null!;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextGroup();
            SelectNextPanel();
            Select();

            AddStep("record selection", () =>
            {
                eagerSelectedIDs = new HashSet<Guid> { ((BeatmapInfo)Carousel.CurrentSelection!).ID };
            });

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddUntilStep("selection cleared", () => Carousel.CurrentSelection == null);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddUntilStep("wait for any selection", () => Carousel.CurrentSelection != null);
                AddStep("record selection", () => eagerSelectedIDs.Add(((BeatmapInfo)Carousel.CurrentSelection!).ID));
            }

            // always returns to same selection as long as it's available.
            AddAssert("selection was remembered", () => eagerSelectedIDs.Count == 1);
        }

        [Test]
        public void TestCarouselRemembersSelectionDifficultySort()
        {
            HashSet<Guid> eagerSelectedIDs = null!;

            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SortBy(SortMode.Difficulty);

            SelectNextGroup();

            AddStep("count selected ID", () =>
            {
                Logger.Log($"selected {((BeatmapInfo)Carousel.CurrentSelection!).ID}");
                eagerSelectedIDs = new HashSet<Guid> { ((BeatmapInfo)Carousel.CurrentSelection!).ID };
            });

            for (int i = 0; i < 5; i++)
            {
                ApplyToFilter("filter all", c => c.SearchText = Guid.NewGuid().ToString());
                AddUntilStep("selection cleared", () => Carousel.CurrentSelection == null);
                ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
                AddUntilStep("wait for any selection", () => Carousel.CurrentSelection != null);
                AddStep("count selected ID", () =>
                {
                    Logger.Log($"selected {((BeatmapInfo)Carousel.CurrentSelection!).ID}");
                    eagerSelectedIDs.Add(((BeatmapInfo)Carousel.CurrentSelection!).ID);
                });
            }

            // always returns to same selection as long as it's available.
            AddAssert("selection was remembered", () => eagerSelectedIDs.Count == 1);
        }

        [Test]
        public void TestCarouselRetainsSelectionFromDifficultySort()
        {
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            BeatmapInfo chosenBeatmap = null!;

            for (int i = 0; i < 3; i++)
            {
                int diff = i;

                AddStep($"select diff {diff}", () => Carousel.CurrentSelection = chosenBeatmap = BeatmapSets[20].Beatmaps[diff]);
                AddUntilStep("selection changed", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Difficulty);
                AddAssert("selection retained", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));

                SortBy(SortMode.Title);
                AddAssert("selection retained", () => Carousel.CurrentSelection, () => Is.EqualTo(chosenBeatmap));
            }
        }

        [Test]
        public void TestCarouselSelectsNextWhenPreviousIsFiltered()
        {
            AddStep("add beatmaps", () =>
            {
                // 10 sets that go osu! -> taiko -> catch -> osu! -> ...
                for (int i = 0; i < 10; i++)
                    BeatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(5, new[] { getRuleset(i) }));
            });
            WaitForDrawablePanels();

            for (int i = 1; i < 10; i++)
            {
                var ruleset = getRuleset(i % 3);
                ApplyToFilter($"Set ruleset to {ruleset.ShortName}", c => c.Ruleset = ruleset);
                WaitForSelection(i, 0);
            }

            static RulesetInfo getRuleset(int index)
            {
                switch (index % 3)
                {
                    default:
                        return new OsuRuleset().RulesetInfo;

                    case 1:
                        return new TaikoRuleset().RulesetInfo;

                    case 2:
                        return new CatchRuleset().RulesetInfo;
                }
            }
        }

        [Test]
        [Ignore("This is intentionally broken in new carousel for code simplicity purposes. Its effects shouldn't be noticeable to the end user.")]
        public void TestCarouselSelectsBackwardsWhenDistanceIsShorter()
        {
            AddStep("add beatmaps", () =>
            {
                // 10 sets that go taiko, osu!, osu!, osu!, taiko, osu!, osu!, osu!, ...
                for (int i = 0; i < 10; i++)
                    BeatmapSets.Add(TestResources.CreateTestBeatmapSetInfo(5, new[] { getRuleset(i) }));
            });
            WaitForDrawablePanels();

            for (int i = 1; i < 9; i += 4)
            {
                int set = i;

                AddStep($"select set {set}", () => Carousel.CurrentSelection = BeatmapSets[set].Beatmaps[0]);
                ApplyToFilter("filter to taiko", c => c.Ruleset = rulesets.AvailableRulesets.ElementAt(1));
                WaitForSelection(set - 1, 0);
                ApplyToFilter("remove filter", c => c.Ruleset = null);
            }

            static RulesetInfo getRuleset(int index)
            {
                switch (index % 4)
                {
                    case 0:
                        return new TaikoRuleset().RulesetInfo;

                    default:
                        return new OsuRuleset().RulesetInfo;
                }
            }
        }
    }
}
