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
    public partial class TestSceneBeatmapBackgroundWedge : SongSelectComponentsTestScene
    {
        private RulesetStore rulesets = null!;
        private TestBeatmapBackgroundWedge backgroundWedge = null!;

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
                    Margin = new MarginPadding { Top = 20 }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 20 },
                    Children = new Drawable[]
                    {
                        backgroundWedge = new TestBeatmapBackgroundWedge
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.6f,
                        },
                    }
                }
            });

            AddSliderStep("change star difficulty", 0, 11.9, 5.55, v =>
            {
                ((BindableDouble)backgroundWedge.DisplayedStars).Value = v;
            });
        }

        [Test]
        public void TestDisplay()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var instance = rulesetInfo.CreateInstance();
                var testBeatmap = createTestBeatmap(rulesetInfo);

                setRuleset(rulesetInfo);

                selectBeatmap(testBeatmap);

                testBeatmapLabels(instance);
            }
        }

        [Test]
        public void TestNoBeatmap()
        {
            selectBeatmap(null);
            AddAssert("check default title", () => backgroundWedge.Info!.TitleLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Title);
            AddAssert("check default artist", () => backgroundWedge.Info!.ArtistLabel.Current.Value == Beatmap.Default.BeatmapInfo.Metadata.Artist);
            AddAssert("check no info labels", () => !backgroundWedge.Info.ChildrenOfType<BeatmapInfoWedge.WedgeInfoText.InfoLabel>().Any());
        }

        private void testBeatmapLabels(Ruleset ruleset)
        {
            AddAssert("check title", () => backgroundWedge.Info!.TitleLabel.Current.Value == $"{ruleset.ShortName}Title");
            AddAssert("check artist", () => backgroundWedge.Info!.ArtistLabel.Current.Value == $"{ruleset.ShortName}Artist");
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(createLongMetadata());
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            Container? containerBefore = null;

            AddStep("set ruleset", () =>
            {
                // wedge content is only refreshed if the ruleset changes, so only wait for load in that case.
                if (!rulesetInfo.Equals(Ruleset.Value))
                    containerBefore = backgroundWedge.DisplayedContent;

                Ruleset.Value = rulesetInfo;
            });

            AddUntilStep("wait for async load", () => backgroundWedge.DisplayedContent != containerBefore);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            Container? containerBefore = null;

            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () =>
            {
                containerBefore = backgroundWedge.DisplayedContent;
                Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b);
                backgroundWedge.Show();
            });

            AddUntilStep("wait for async load", () => backgroundWedge.DisplayedContent != containerBefore);
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

        private partial class TestBeatmapBackgroundWedge : BeatmapBackgroundWedge
        {
            public new Container? DisplayedContent => base.DisplayedContent;
            public new WedgeInfoText? Info => base.Info;
        }

        private class TestHitObject : ConvertHitObject;
    }
}
