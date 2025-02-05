// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapCarouselSetPanel : ThemeComparisonTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private BeatmapSetInfo beatmapSet = null!;

        public TestSceneBeatmapCarouselSetPanel()
            : base(false)
        {
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("set beatmap", () =>
            {
                beatmapSet = beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => b.OnlineID == 241526)
                             ?? beatmaps.GetAllUsableBeatmapSets().FirstOrDefault(b => !b.Protected)
                             ?? TestResources.CreateTestBeatmapSetInfo();
                CreateThemedContent(OverlayColourScheme.Aquamarine);
            });
        }

        [Test]
        public void TestRandomBeatmap()
        {
            AddStep("random beatmap", () =>
            {
                beatmapSet = beatmaps.GetAllUsableBeatmapSets().OrderBy(_ => RNG.Next()).First();
                CreateThemedContent(OverlayColourScheme.Aquamarine);
            });
        }

        protected override Drawable CreateContent()
        {
            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0f, 5f),
                Children = new Drawable[]
                {
                    new BeatmapSetPanel
                    {
                        Item = new CarouselItem(beatmapSet) { IsGroupSelectionTarget = true }
                    },
                    new BeatmapSetPanel
                    {
                        Item = new CarouselItem(beatmapSet) { IsGroupSelectionTarget = true },
                        KeyboardSelected = { Value = true }
                    },
                    new BeatmapSetPanel
                    {
                        Item = new CarouselItem(beatmapSet) { IsGroupSelectionTarget = true },
                        CustomExpanded = { Value = true }
                    },
                    new BeatmapSetPanel
                    {
                        Item = new CarouselItem(beatmapSet) { IsGroupSelectionTarget = true },
                        KeyboardSelected = { Value = true },
                        CustomExpanded = { Value = true }
                    },
                }
            };
        }
    }
}
