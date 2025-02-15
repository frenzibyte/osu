// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Screens.Select;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectTopWedges : SongSelectComponentsTestScene
    {
        private RulesetStore rulesets = null!;
        private TestBeatmapMainWedge mainWedge = null!;
        private BeatmapDifficultyWedge diffWedge = null!;

        private readonly List<IBeatmap> beatmaps = new List<IBeatmap>();

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset mods", () => SelectedMods.SetDefault());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                // This exists only to make the wedge more visible in the test scene
                new Box
                {
                    Y = -20,
                    Colour = Colour4.Cornsilk.Darken(0.2f),
                    Height = 250,
                    Width = 0.65f,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding { Top = 20, Left = -10 }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 20 },
                    Children = new Drawable[]
                    {
                        mainWedge = new TestBeatmapMainWedge(),
                        diffWedge = new BeatmapDifficultyWedge(),
                    },
                }
            });

            AddSliderStep("change star difficulty", 0, 11.9, 4.18, v =>
            {
                ((BindableDouble)mainWedge.DisplayedStars).Value = v;
                ((BindableDouble)diffWedge.DisplayedStars).Value = v;
            });
        }

        [Test]
        public void TestRulesetChange()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var instance = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                beatmaps.Add(testBeatmap);

                setRuleset(rulesetInfo);

                selectBeatmap(testBeatmap);

                testBeatmapLabels(instance);
            }
        }

        [Test]
        public void TestWedgeVisibility()
        {
            AddStep("hide", () => { mainWedge.Hide(); });
            AddWaitStep("wait for hide", 3);
            AddAssert("check visibility", () => mainWedge.Alpha == 0);
            AddStep("show", () => { mainWedge.Show(); });
            AddWaitStep("wait for show", 1);
            AddAssert("check visibility", () => mainWedge.Alpha > 0);
        }

        private void testBeatmapLabels(Ruleset ruleset)
        {
            AddAssert("check title", () => mainWedge.Content!.TitleLabel.Current.Value == $"{ruleset.ShortName}Title");
            AddAssert("check artist", () => mainWedge.Content!.ArtistLabel.Current.Value == $"{ruleset.ShortName}Artist");
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(createLongMetadata());
        }

        [Test]
        public void TestNullBeatmapWithBackground()
        {
            selectBeatmap(null);
            AddAssert("check default title", () => mainWedge.Content!.TitleLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => mainWedge.Content!.ArtistLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check no info labels", () => !mainWedge.Content.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any());
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            Container? containerBefore = null;

            AddStep("set ruleset", () =>
            {
                // wedge content is only refreshed if the ruleset changes, so only wait for load in that case.
                if (!rulesetInfo.Equals(Ruleset.Value))
                    containerBefore = mainWedge.DisplayedContent;

                Ruleset.Value = rulesetInfo;
            });

            AddUntilStep("wait for async load", () => mainWedge.DisplayedContent != containerBefore);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            Container? containerBefore = null;

            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                containerBefore = mainWedge.DisplayedContent;
                Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b);
                mainWedge.Show();
            });

            AddUntilStep("wait for async load", () => mainWedge.DisplayedContent != containerBefore);
        }

        private IBeatmap createTestBeatmap(RulesetInfo ruleset)
        {
            List<HitObject> objects = new List<HitObject>();
            for (double i = 0; i < 50000; i += 1000)
                objects.Add(new TestHitObject { StartTime = i });

            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Author = { Username = $"{ruleset.ShortName}Author" },
                        Artist = $"{ruleset.ShortName}Artist",
                        Source = $"{ruleset.ShortName}Source",
                        Title = $"{ruleset.ShortName}Title"
                    },
                    Ruleset = ruleset,
                    StarRating = 6,
                    DifficultyName = $"{ruleset.ShortName}Version",
                    Difficulty = new BeatmapDifficulty()
                },
                HitObjects = objects
            };
        }

        private IBeatmap createLongMetadata()
        {
            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Author = { Username = "WWWWWWWWWWWWWWW" },
                        Artist = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Artist",
                        Source = "Verrrrry long Source",
                        Title = "Verrrrry long Title"
                    },
                    DifficultyName = "Verrrrrrrrrrrrrrrrrrrrrrrrrrrrry long Version",
                    Status = BeatmapOnlineStatus.Graveyard,
                },
            };
        }

        private partial class TestBeatmapMainWedge : BeatmapMainWedge
        {
            public new Container? DisplayedContent => base.DisplayedContent;
            public new BeatmapMainWedgeContent? Content => base.Content;
        }

        private class TestHitObject : ConvertHitObject;
    }
}
