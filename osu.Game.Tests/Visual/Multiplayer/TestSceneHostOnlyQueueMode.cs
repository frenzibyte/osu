// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneHostOnlyQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.HostOnly;

        [Test]
        [Repeat(100)]
        /*
         * TearDown : System.TimeoutException : "wait for ongoing operation to complete" timed out
         *   --TearDown
         *      at osu.Framework.Testing.Drawables.Steps.UntilStepButton.<>c__DisplayClass11_0.<.ctor>b__0()
         *      at osu.Framework.Testing.Drawables.Steps.StepButton.PerformStep(Boolean userTriggered)
         *      at osu.Framework.Testing.TestScene.runNextStep(Action onCompletion, Action`1 onError, Func`2 stopCondition)
         */
        public void TestItemStillSelectedAfterChangeToSameBeatmap()
        {
            selectNewItem(() => InitialBeatmap);

            AddUntilStep("playlist item still selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        [Repeat(100)] // See above
        public void TestItemStillSelectedAfterChangeToOtherBeatmap()
        {
            selectNewItem(() => OtherBeatmap);

            AddUntilStep("playlist item still selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        private void selectNewItem(Func<BeatmapInfo> beatmap)
        {
            AddUntilStep("wait for playlist panels to load", () =>
            {
                var queueList = this.ChildrenOfType<MultiplayerQueueList>().Single();
                return queueList.ChildrenOfType<DrawableRoomPlaylistItem>().Count() == queueList.Items.Count;
            });

            AddStep("click edit button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistEditButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.BeatmapSetsLoaded);

            BeatmapInfo otherBeatmap = null;

            AddUntilStep("wait for ongoing operation to complete", () =>
            {
                Logger.Log($"{nameof(TestSceneHostOnlyQueueMode)}.{nameof(selectNewItem)}(): Current screen is {CurrentScreen}, sub screen is {CurrentSubScreen}.");
                return !(CurrentScreen as OnlinePlayScreen).ChildrenOfType<OngoingOperationTracker>().Single().InProgress.Value;
            });

            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(otherBeatmap = beatmap()));

            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
            AddUntilStep("selected item is new beatmap", () => (CurrentSubScreen as MultiplayerMatchSubScreen)?.SelectedItem.Value?.Beatmap.OnlineID == otherBeatmap.OnlineID);
        }

        private void addItem(Func<BeatmapInfo> beatmap)
        {
            AddStep("click add button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen.AddItemButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.BeatmapSetsLoaded);
            AddUntilStep("wait for ongoing operation to complete", () => !(CurrentScreen as OnlinePlayScreen).ChildrenOfType<OngoingOperationTracker>().Single().InProgress.Value);
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(beatmap()));
            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
        }
    }
}
