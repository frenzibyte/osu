// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSetV2;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapInfoOverlay : OsuTestScene
    {
        private BeatmapInfoOverlay beatmapInfoOverlay;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            ((DummyAPIAccess)API).HandleRequest += r =>
            {
                switch (r)
                {
                    case PostBeatmapFavouriteRequest postBeatmapFavouriteRequest:
                        Scheduler.AddDelayed(() => postBeatmapFavouriteRequest.TriggerSuccess(), 500);
                        return true;
                }

                return false;
            };

            Child = beatmapInfoOverlay = new BeatmapInfoOverlay();
        });

        [Test]
        public void TestNullBeatmapSet()
        {
            AddStep("show null", () => beatmapInfoOverlay.ShowBeatmapSet(null));
        }

        [Test]
        public void TestLocalBeatmapSet([Values(false, true)] bool hasExplicitContent)
        {
            foreach (var status in Enum.GetValues(typeof(BeatmapSetOnlineStatus)).Cast<BeatmapSetOnlineStatus>())
                AddStep($"show {status.ToString().ToLower()}", () => beatmapInfoOverlay.ShowBeatmapSet(createTestBeatmapSet(status, hasExplicitContent)));
        }

        private int setID;

        private APIBeatmapSet createTestBeatmapSet(BeatmapSetOnlineStatus status, bool hasExplicitContent)
        {
            var set = new APIBeatmapSet
            {
                OnlineID = ++setID,
                Title = "quaver",
                Artist = "dj TAKA",
                AuthorString = "Sotarks",
                Source = "REFLEC BEAT limelight",
                Tags = "risk junk beatmania iidx 19 lincle jubeat saucer jukebeat",
                Status = status,
                HasExplicitContent = hasExplicitContent,
                HasFavourited = true,
                FavouriteCount = 2593,
                Genre = new BeatmapSetOnlineGenre { Name = "Video Game (Instrumental)" },
                Language = new BeatmapSetOnlineLanguage { Name = "Instrumental" },
                Covers = new BeatmapSetOnlineCovers
                {
                    Cover = "https://s3-alpha-sig.figma.com/img/d95c/56af/cc030fa23122fcb38db25d02288a977d?Expires=1636329600&Signature=bmpZcUhqCWXY8dqyEC5u5~Ev0L04BoxxYaBlyZiI9MyNJqj41S2869DOicjxLejNJtig5kuFaXpv31oB5qUAcve4B5xE0CVKnsKWZWHJag-N5zdIXvCciGYEPnG-YGqZJ-oICvS-Byhae6PcC0sAu0IwIHTEV-TJIhj3KTHWFEFy12fKMrElsas8BmlPF7niy7cbY3OQjcIXmeoiYUdGTBNxnLOIWhHmnQM-VzUQa~cJEATPmPAJFRw8Lolf~we754pDGwMP~joIiX0palLEbipEo~3iU4MfXoTjpYqMlE2QiH3IRwpgjD5~6rTZshv2XD8-Kkw~eka8jejpQ976xw__&Key-Pair-Id=APKAINTVSUGEWH5XD5UA"
                },
                Submitted = new DateTime(2018, 11, 4, 0, 0, 0),
                Ranked = new DateTime(2019, 1, 6, 0, 0, 0),
                Ratings = new[]
                {
                    0,
                    1320,
                    193,
                    157,
                    290,
                    359,
                    398,
                    773,
                    1477,
                    2792,
                    15922
                },
            };

            set.Beatmaps = Enumerable.Range(0, 32).Select(i =>
            {
                var beatmap = createTestBeatmap(set, i);
                beatmap.RulesetID = i % 4;
                return beatmap;
            }).ToArray();

            return set;
        }

        private static APIBeatmap createTestBeatmap(APIBeatmapSet beatmapSet, int index) => new APIBeatmap
        {
            OnlineID = 1000 + index,
            BeatmapSet = beatmapSet,
            DifficultyName = "Very Long Difficulty Name",
            ApproachRate = 6.78f + index * 0.09f,
            CircleSize = 4f + (index / 5) * 1,
            DrainRate = 5.29f + index * 0.2f,
            OverallDifficulty = 4.6f + index * 0.19f,
            StarRating = 1.50 + index * 0.525,
            Length = 203000 + index * 1234,
            BPM = 140 + index * 5,
            CircleCount = 325 + index * 4,
            SliderCount = 140 + index * 2,
            PlayCount = 3945834 + index * 164348,
            PassCount = 3274821 + index * 73234,
            FailTimes = new APIFailTimes
            {
                Fails = new[]
                {
                    1208,
                    144,
                    135,
                    933,
                    646,
                    186,
                    2543,
                    1913,
                    392571,
                    2683758,
                    2056339,
                    6948934,
                    343499,
                    126522,
                    349490,
                    729544,
                    1027766,
                    1409685,
                    1377226,
                    478937,
                    224993,
                    285757,
                    46954,
                    31898,
                    26637,
                    131522,
                    92903,
                    140571,
                    25960,
                    165,
                    145,
                    156,
                    160,
                    143,
                    159,
                    161,
                    10076,
                    36669,
                    36215,
                    20036,
                    43462,
                    124668,
                    81752,
                    9792,
                    2909,
                    5338,
                    1046,
                    769,
                    8945,
                    12281,
                    5424,
                    3685,
                    14123,
                    26408,
                    16485,
                    5824,
                    1915,
                    1781,
                    494,
                    4262,
                    1896,
                    155,
                    149,
                    197,
                    153,
                    151,
                    155,
                    365,
                    26386,
                    89990,
                    61557,
                    36736,
                    18618,
                    39772,
                    20801,
                    3528,
                    1525,
                    2882,
                    3944,
                    12139,
                    8329,
                    12280,
                    15101,
                    3929,
                    14595,
                    9096,
                    19166,
                    35912,
                    22182,
                    32425,
                    85420,
                    25018,
                    12053,
                    26222,
                    12151,
                    123739,
                    28766,
                    4664,
                    26386,
                    118075
                },
            },
        };
    }
}
