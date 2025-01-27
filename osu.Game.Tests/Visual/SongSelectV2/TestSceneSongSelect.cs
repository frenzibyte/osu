// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2.Footer;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelect : ScreenTestScene
    {
        [Cached]
        private readonly ScreenFooter screenScreenFooter;

        [Cached]
        private readonly OsuLogo logo;

        private BeatmapManager manager = null!;
        private MusicController music = null!;

        public TestSceneSongSelect()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = screenScreenFooter = new ScreenFooter
                    {
                        OnBack = () => Stack.CurrentScreen.Exit(),
                    },
                },
                logo = new OsuLogo
                {
                    Alpha = 0f,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());

            Dependencies.Cache(music = new MusicController());

            // required to get bindables attached
            Add(music);
            Add(beatmapStore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load screen", () => Stack.Push(new Screens.SelectV2.SongSelectV2()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen is Screens.SelectV2.SongSelectV2 songSelect && songSelect.IsLoaded);

            AddStep("import test beatmap", () =>
            {
                var beatmapSet = manager.Import(new ImportTask(TestResources.GetTestBeatmapForImport())).GetResultSafely()!;

                beatmapSet.PerformWrite(b =>
                {
                    b.Status = BeatmapOnlineStatus.Ranked;
                    b.Beatmaps.First().Status = BeatmapOnlineStatus.Ranked;
                });

                Beatmap.Value = manager.GetWorkingBeatmap(beatmapSet.Value.Beatmaps.First());
            });

            AddStep("hook up api", () =>
            {
                var dummyAPI = (DummyAPIAccess)API;
                dummyAPI.LocalUser.Value.IsSupporter = true;
                dummyAPI.HandleRequest = r =>
                {
                    switch (r)
                    {
                        case GetScoresRequest scoresRequest:
                            scoresRequest.TriggerSuccess(createScores());
                            return true;

                        default:
                            return false;
                    }
                };
            });
        }

        #region Footer

        [Test]
        public void TestMods()
        {
            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });
            AddStep("two mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock() });
            AddStep("three mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime() });
            AddStep("four mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic() });
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });

            AddStep("modified", () => SelectedMods.Value = new List<Mod> { new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + one", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + two", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + three", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + four", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDifficultyAdjust(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });
        }

        [Test]
        public void TestShowOptions()
        {
            AddStep("enable options", () =>
            {
                var optionsButton = this.ChildrenOfType<ScreenFooterButton>().Last();

                optionsButton.Enabled.Value = true;
                optionsButton.TriggerClick();
            });
        }

        [Test]
        public void TestState()
        {
            AddToggleStep("set options enabled state", state => this.ChildrenOfType<ScreenFooterButton>().Last().Enabled.Value = state);
        }

        // add these test cases when functionality is implemented.
        // [Test]
        // public void TestFooterRandom()
        // {
        //     AddStep("press F2", () => InputManager.Key(Key.F2));
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRandomViaMouse()
        // {
        //     AddStep("click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //     });
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewind()
        // {
        //     AddStep("press Shift+F2", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.PressKey(Key.F2);
        //         InputManager.ReleaseKey(Key.F2);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaShiftMouseLeft()
        // {
        //     AddStep("shift + click button", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaMouseRight()
        // {
        //     AddStep("right click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Right);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }

        [Test]
        public void TestOverlayPresent()
        {
            AddStep("Press F1", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ScreenFooterButtonMods>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Overlay visible", () => this.ChildrenOfType<ModSelectOverlay>().Single().State.Value == Visibility.Visible);
            AddStep("Hide", () => this.ChildrenOfType<ModSelectOverlay>().Single().Hide());
        }

        #endregion

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.ShowFooter)
            {
                screenScreenFooter.Show();
                screenScreenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenScreenFooter.Hide();
                screenScreenFooter.SetButtons(Array.Empty<ScreenFooterButton>());
            }
        }

        private ulong onlineID = 1;

        private APIScoresCollection createScores()
        {
            var scores = new APIScoresCollection
            {
                Scores = new List<SoloScoreInfo>
                {
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 6602580,
                            Username = @"waaiiru",
                            CountryCode = CountryCode.ES,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                            new APIMod { Acronym = new OsuModFlashlight().Acronym },
                            new APIMod { Acronym = new OsuModHardRock().Acronym },
                        },
                        Rank = ScoreRank.XH,
                        PP = 200,
                        MaxCombo = 1234,
                        TotalScore = 1000000,
                        Accuracy = 1,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 4608074,
                            Username = @"Skycries",
                            CountryCode = CountryCode.BR,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                            new APIMod { Acronym = new OsuModFlashlight().Acronym },
                        },
                        Rank = ScoreRank.S,
                        PP = 190,
                        MaxCombo = 1234,
                        TotalScore = 823478,
                        Accuracy = 0.9997,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 1014222,
                            Username = @"eLy",
                            CountryCode = CountryCode.JP,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                            new APIMod { Acronym = new OsuModHidden().Acronym },
                        },
                        Rank = ScoreRank.B,
                        PP = 180,
                        MaxCombo = 1234,
                        TotalScore = 934567,
                        Accuracy = 0.9854,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 1541390,
                            Username = @"Toukai",
                            CountryCode = CountryCode.CA,
                        },
                        Mods = new[]
                        {
                            new APIMod { Acronym = new OsuModDoubleTime().Acronym },
                        },
                        Rank = ScoreRank.C,
                        PP = 170,
                        MaxCombo = 1234,
                        TotalScore = 723456,
                        Accuracy = 0.8765,
                        Ranked = true,
                    },
                    new SoloScoreInfo
                    {
                        EndedAt = DateTimeOffset.Now,
                        ID = onlineID++,
                        User = new APIUser
                        {
                            Id = 7151382,
                            Username = @"Mayuri Hana",
                            CountryCode = CountryCode.TH,
                        },
                        Rank = ScoreRank.D,
                        PP = 160,
                        MaxCombo = 1234,
                        TotalScore = 123456,
                        Accuracy = 0.6543,
                        Ranked = true,
                    },
                }
            };

            double maxScore = scores.Scores.Max(s => s.TotalScore);
            scores.Scores = Enumerable.Repeat(scores.Scores, 10).SelectMany(s => s).ToList();

            const int initial_great_count = 2000;
            const int initial_tick_count = 100;
            const int initial_slider_end_count = 500;

            int greatCount = initial_great_count;
            int tickCount = initial_tick_count;
            int sliderEndCount = initial_slider_end_count;

            foreach (var (score, index) in scores.Scores.Select((s, i) => (s, i)))
            {
                HitResult sliderEndResult = index % 2 == 0 ? HitResult.SliderTailHit : HitResult.SmallTickHit;

                score.Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Great, greatCount },
                    { HitResult.LargeTickHit, tickCount },
                    { HitResult.Ok, RNG.Next(100) },
                    { HitResult.Meh, RNG.Next(100) },
                    { HitResult.Miss, initial_great_count - greatCount },
                    { HitResult.LargeTickMiss, initial_tick_count - tickCount },
                    { sliderEndResult, sliderEndCount },
                };

                // Some hit results, including SliderTailHit and SmallTickHit, are only displayed
                // when the maximum number is known
                score.MaximumStatistics = new Dictionary<HitResult, int>
                {
                    { sliderEndResult, initial_slider_end_count },
                };

                greatCount -= 100;
                tickCount -= RNG.Next(1, 5);
                sliderEndCount -= 20;
            }

            return scores;
        }
    }
}
