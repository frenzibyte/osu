// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckTitleMarkersTest
    {
        private CheckTitleMarkers check = null!;

        private IBeatmap beatmap = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckTitleMarkers();

            beatmap = new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Egao no Kanata",
                        TitleUnicode = "エガオノカナタ"
                    }
                }
            };
        }

        [Test]
        public void TestNoTitleMarkers()
        {
            performTest(string.Empty, string.Empty);
        }

        [Test]
        public void TestTvSizeMarker()
        {
            performTest("(TV Size)", "(TV Size)");
            performTest("(Tv size)", "(TV Size)");
            performTest("[TV Size]", "(TV Size)");
            performTest("(TV Ver.)", "(TV Size)");
            performTest("(TV Ver)", "(TV Size)");
        }

        [Test]
        public void TestGameVerMarker()
        {
            performTest("(Game Ver.)", "(Game Ver.)");
            performTest("(Game ver.)", "(Game Ver.)");
            performTest("[Game Ver.]", "(Game Ver.)");
            performTest("(Game Size)", "(Game Ver.)");
            performTest("(Game Ver)", "(Game Ver.)");
        }

        [Test]
        public void TestShortVerMarker()
        {
            performTest("(Short Ver.)", "(Short Ver.)");
            performTest("(Short ver.)", "(Short Ver.)");
            performTest("[Short Ver.]", "(Short Ver.)");
            performTest("(Short Size)", "(Short Ver.)");
            performTest("(Short Ver)", "(Short Ver.)");
        }

        [Test]
        public void TestCutVerMarker()
        {
            performTest("(Cut Ver.)", "(Cut Ver.)");
            performTest("(Cut ver.)", "(Cut Ver.)");
            performTest("[Cut Ver.]", "(Cut Ver.)");
            performTest("(Cut Size)", "(Cut Ver.)");
            performTest("(Cut Ver)", "(Cut Ver.)");
        }

        [Test]
        public void TestSpedUpVerMarker()
        {
            performTest("(Sped Up Ver.)", "(Sped Up Ver.)");
            performTest("(Sped Up ver.)", "(Sped Up Ver.)");
            performTest("[Sped Up Ver.]", "(Sped Up Ver.)");
            performTest("(Speed Up Ver.)", "(Sped Up Ver.)");
            performTest("(Sped Up Ver)", "(Sped Up Ver.)");
        }

        [Test]
        public void TestNightcoreMixMarker()
        {
            performTest("(Nightcore Mix)", "(Nightcore Mix)");
            performTest("(Nightcore mix)", "(Nightcore Mix)");
            performTest("[Nightcore Mix]", "(Nightcore Mix)");
            performTest("(Nightcore Ver.)", "(Nightcore Mix)");
            performTest("(Nightcore Ver)", "(Nightcore Mix)");
            performTest("(Night Core Mix)", "(Nightcore Mix)");
            performTest("(Night Core Ver.)", "(Nightcore Mix)");
        }

        [Test]
        public void TestSpedUpCutVerMarker()
        {
            performTest("(Sped Up & Cut Ver.)", "(Sped Up & Cut Ver.)");
            performTest("(Sped up & cut ver.)", "(Sped Up & Cut Ver.)");
            performTest("[Sped Up & Cut Ver.]", "(Sped Up & Cut Ver.)");
            performTest("(Speed Up & Cut Ver.)", "(Sped Up & Cut Ver.)");
            performTest("(Sped Up & Cut Size)", "(Sped Up & Cut Ver.)");
            performTest("(SpedUp & Cut Ver.)", "(Sped Up & Cut Ver.)");
            performTest("(Sped Up & Cut Ver)", "(Sped Up & Cut Ver.)");
        }

        [Test]
        public void TestNightcoreCutVerMarker()
        {
            performTest("(Nightcore & Cut Ver.)", "(Nightcore & Cut Ver.)");
            performTest("(Nightcore & cut ver.)", "(Nightcore & Cut Ver.)");
            performTest("[Nightcore & Cut Ver.]", "(Nightcore & Cut Ver.)");
            performTest("(Night Core & Cut Ver.)", "(Nightcore & Cut Ver.)");
            performTest("(Nightcore & Cut Ver)", "(Nightcore & Cut Ver.)");
        }

        private void performTest(string marker, string expected)
        {
            performTest(marker, expected, false);
            performTest(marker, expected, true);
        }

        private void performTest(string marker, string expected, bool hasRomanisedTitle)
        {
            beatmap.BeatmapInfo.Metadata.Title = "Egao no Kanata " + marker;

            if (hasRomanisedTitle)
                beatmap.BeatmapInfo.Metadata.TitleUnicode = "エガオノカナタ " + marker;
            else
                beatmap.BeatmapInfo.Metadata.TitleUnicode = beatmap.BeatmapInfo.Metadata.Title;

            var issues = check.Run(getContext(beatmap)).ToList();

            if (marker == expected)
                CollectionAssert.IsEmpty(issues);
            else
            {
                if (hasRomanisedTitle)
                {
                    Assert.That(issues, Has.Count.EqualTo(2));
                    Assert.That(issues[0].Arguments[0], Is.EqualTo("Title"));
                    Assert.That(issues[0].Arguments[1], Is.EqualTo(expected));
                    Assert.That(issues[1].Arguments[0], Is.EqualTo("Romanised title"));
                    Assert.That(issues[1].Arguments[1], Is.EqualTo(expected));
                }
                else
                {
                    Assert.That(issues, Has.Count.EqualTo(1));
                    Assert.That(issues[0].Arguments[0], Is.EqualTo("Title"));
                    Assert.That(issues[0].Arguments[1], Is.EqualTo(expected));
                }
            }
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
