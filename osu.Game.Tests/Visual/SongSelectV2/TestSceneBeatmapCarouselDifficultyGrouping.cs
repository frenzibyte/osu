// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    [TestFixture]
    public partial class TestSceneBeatmapCarouselDifficultyGrouping : BeatmapCarouselTestScene
    {
        [SetUpSteps]
        public void SetUpSteps()
        {
            RemoveAllBeatmaps();
            CreateCarousel();

            SortBy(SortMode.Difficulty);
            GroupBy(GroupMode.Difficulty);

            AddBeatmaps(10, 3);
            WaitForDrawablePanels();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionMouse()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            CheckNoSelection();

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();
        }

        [Test]
        public void TestOpenCloseGroupWithNoSelectionKeyboard()
        {
            AddAssert("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            CheckNoSelection();

            SelectNextPanel();
            Select();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));
            AddAssert("keyboard selected is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
            CheckNoSelection();

            Select();
            AddUntilStep("no beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.Zero);
            AddAssert("keyboard selected is collapsed", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);
            CheckNoSelection();
        }

        [Test]
        public void TestCarouselRemembersSelection()
        {
            SelectNextGroup();

            object? selection = null;

            AddStep("store drawable selection", () => selection = GetSelectedPanel()?.Item?.Model);

            CheckHasSelection();
            AddAssert("drawable selection non-null", () => selection, () => Is.Not.Null);
            AddAssert("drawable selection matches carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));

            RemoveAllBeatmaps();
            AddUntilStep("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddBeatmaps(3);
            WaitForDrawablePanels();

            CheckHasSelection();
            AddAssert("no drawable selection", GetSelectedPanel, () => Is.Null);

            AddStep("add previous selection", () => BeatmapSets.Add(((BeatmapInfo)selection!).BeatmapSet!));

            AddAssert("selection matches original carousel selection", () => selection, () => Is.EqualTo(Carousel.CurrentSelection));
            AddUntilStep("drawable selection restored", () => GetSelectedPanel()?.Item?.Model, () => Is.EqualTo(selection));
            AddAssert("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("carousel item not visible", GetSelectedPanel, () => Is.Null);

            ClickVisiblePanel<PanelGroup>(0);
            AddUntilStep("carousel item is visible", () => GetSelectedPanel()?.Item?.IsVisible, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeaderKeyboard()
        {
            SelectNextGroup();
            WaitForGroupSelection(0, 0);

            SelectPrevPanel();
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            SelectPrevGroup();

            WaitForGroupSelection(0, 0);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            SelectPrevGroup();

            WaitForGroupSelection(0, 0);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);
        }

        [Test]
        public void TestGroupSelectionOnHeaderMouse()
        {
            SelectNextGroup();
            WaitForGroupSelection(0, 0);

            AddAssert("keyboard selected panel is beatmap", GetKeyboardSelectedPanel, Is.TypeOf<PanelBeatmap>);
            AddAssert("selected panel is beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmap>);

            ClickVisiblePanel<PanelGroup>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroup>);
            AddAssert("keyboard selected panel is contracted", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.False);

            ClickVisiblePanel<PanelGroup>(0);
            AddAssert("keyboard selected panel is group", GetKeyboardSelectedPanel, Is.TypeOf<PanelGroup>);
            AddAssert("keyboard selected panel is expanded", () => GetKeyboardSelectedPanel()?.Expanded.Value, () => Is.True);

            AddAssert("selected panel is still beatmap", GetSelectedPanel, Is.TypeOf<PanelBeatmap>);
        }

        [Test]
        public void TestKeyboardSelection()
        {
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            SelectNextPanel();
            CheckNoSelection();

            // open first group
            Select();
            CheckNoSelection();
            AddUntilStep("some beatmaps visible", () => Carousel.ChildrenOfType<PanelBeatmap>().Count(p => p.Alpha > 0), () => Is.GreaterThan(0));

            SelectNextPanel();
            Select();
            WaitForGroupSelection(0, 0);

            SelectNextGroup();
            WaitForGroupSelection(0, 1);

            SelectNextGroup();
            WaitForGroupSelection(0, 2);

            SelectPrevGroup();
            WaitForGroupSelection(0, 1);

            SelectPrevGroup();
            WaitForGroupSelection(0, 0);

            SelectPrevGroup();
            WaitForGroupSelection(2, 9);
        }

        [Test]
        public void TestInputHandlingWithinGaps()
        {
            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            // Clicks just above the first group panel should not actuate any action.
            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2 + 1)));

            AddAssert("no beatmaps visible", () => !GetVisiblePanels<PanelBeatmap>().Any());

            ClickVisiblePanelWithOffset<PanelGroup>(0, new Vector2(0, -(PanelGroup.HEIGHT / 2)));

            AddUntilStep("wait for beatmaps visible", () => GetVisiblePanels<PanelBeatmap>().Any());
            CheckNoSelection();

            // Beatmap panels expand their selection area to cover holes from spacing.
            ClickVisiblePanelWithOffset<PanelBeatmap>(0, new Vector2(0, -(CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 0);

            ClickVisiblePanelWithOffset<PanelBeatmap>(1, new Vector2(0, (CarouselItem.DEFAULT_HEIGHT / 2 + 1)));
            WaitForGroupSelection(0, 1);
        }

        [Test]
        public void TestBasicFiltering()
        {
            WaitForDrawablePanels();

            SelectNextPanel();
            Select();

            ApplyToFilter("filter", c => c.SearchText = BeatmapSets[2].Metadata.Title);
            WaitForFiltering();

            CheckVisibleGroupsCount(3);
            CheckVisibleBeatmapsCount(3);
            WaitForSelection(2, 1);

            for (int i = 0; i < 5; i++)
                SelectNextPanel();

            Select();
            SelectNextPanel();
            Select();

            WaitForSelection(2, 2);

            ApplyToFilter("remove filter", c => c.SearchText = string.Empty);
            WaitForFiltering();

            CheckVisibleGroupsCount(3);
            CheckVisibleBeatmapsCount(30);
        }

        [Test]
        public void TestSelectionChangesWithFiltering()
        {
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
        public void TestCarouselRemembersSelectionWithFiltering()
        {
            HashSet<Guid> eagerSelectedIDs = null!;

            RemoveAllBeatmaps();
            AddBeatmaps(50, 3);
            WaitForDrawablePanels();

            SelectNextGroup();

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
    }
}
