// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectTopWedges : SongSelectComponentsTestScene
    {
        private RulesetStore rulesets = null!;
        private BeatmapBackgroundWedge backgroundWedge = null!;
        private BeatmapContentWedge contentWedge = null!;

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
                        backgroundWedge = new BeatmapBackgroundWedge
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.6f,
                        },
                        contentWedge = new BeatmapContentWedge
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.55f,
                        },
                    }
                }
            });

            AddSliderStep("change star difficulty", 0, 11.9, 5.55, v =>
            {
                ((BindableDouble)backgroundWedge.DisplayedStars).Value = v;
                ((BindableDouble)contentWedge.DisplayedStars).Value = v;
            });
        }

        [Test]
        public void TestDisplay()
        {
            selectBeatmap(Beatmap.Value.Beatmap);

            AddWaitStep("wait for select", 3);

            foreach (var rulesetInfo in rulesets.AvailableRulesets)
            {
                var testBeatmap = createTestBeatmap(rulesetInfo);

                setRuleset(rulesetInfo);
                selectBeatmap(testBeatmap);
            }
        }

        [Test]
        public void TestNoBeatmap()
        {
            selectBeatmap(null);
        }

        [Test]
        public void TestTruncation()
        {
            selectBeatmap(createLongMetadata());
        }

        private void setRuleset(RulesetInfo rulesetInfo)
        {
            AddStep("set ruleset", () => Ruleset.Value = rulesetInfo);
        }

        private void selectBeatmap(IBeatmap? b)
        {
            AddStep($"select {b?.Metadata.Title ?? "null"} beatmap", () => Beatmap.Value = b == null ? Beatmap.Default : CreateWorkingBeatmap(b));
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

        private class TestHitObject : ConvertHitObject;
    }
}
