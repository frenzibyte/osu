﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text;
using Discord;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Users;
using LogLevel = osu.Framework.Logging.LogLevel;

namespace osu.Desktop
{
    internal partial class DiscordRichPresence : Component
    {
        private const long client_id = 1332169862365184061;

        private Discord.Discord discord = null!;
        private ActivityManager activityManager = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private LoginOverlay? login { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private LocalUserStatisticsProvider statisticsProvider { get; set; } = null!;

        private IBindable<DiscordRichPresenceMode> privacyMode = null!;
        private IBindable<UserStatus> userStatus = null!;
        private IBindable<UserActivity?> userActivity = null!;

        private Activity activity = new Activity
        {
            Assets = new ActivityAssets { LargeImage = "osu_logo_lazer" },
            Secrets = new ActivitySecrets(), // todo: may have broke this?
        };

        private IBindable<APIUser>? user;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics session)
        {
            privacyMode = config.GetBindable<DiscordRichPresenceMode>(OsuSetting.DiscordRichPresence);
            userStatus = config.GetBindable<UserStatus>(OsuSetting.UserOnlineStatus);
            userActivity = session.GetBindable<UserActivity?>(Static.UserOnlineActivity);

            discord = new Discord.Discord(client_id, (ulong)CreateFlags.Default);
            discord.SetLogHook(Discord.LogLevel.Debug, (level, message) =>
            {
                switch (level)
                {
                    case Discord.LogLevel.Debug:
                        Logger.Log($"Discord RPC log: {message}", LoggingTarget.Runtime, LogLevel.Debug);
                        break;

                    case Discord.LogLevel.Info:
                        Logger.Log($"Discord RPC log: {message}", LoggingTarget.Runtime, LogLevel.Verbose);
                        break;

                    case Discord.LogLevel.Warn:
                        Logger.Log($"Discord RPC log: {message}", LoggingTarget.Runtime, LogLevel.Important);
                        break;

                    case Discord.LogLevel.Error:
                        Logger.Log($"Discord RPC log: {message}", LoggingTarget.Runtime, LogLevel.Error);
                        break;
                }
            });

            activityManager = discord.GetActivityManager();

            try
            {
                activityManager.RegisterCommand();
                // client.Subscribe(EventType.Join);
                activityManager.OnActivityJoin += onActivityJoin;
            }
            catch (Exception ex)
            {
                // This is known to fail in at least the following sandboxed environments:
                // - macOS (when packaged as an app bundle)
                // - flatpak (see: https://github.com/flathub/sh.ppy.osu/issues/170)
                // There is currently no better way to do this offered by Discord, so the best we can do is simply ignore it for now.
                Logger.Log($"Failed to register Discord URI scheme: {ex}");
            }

            // client.Initialize();
            Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);
            schedulePresenceUpdate();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user = api.LocalUser.GetBoundCopy();

            ruleset.BindValueChanged(_ => schedulePresenceUpdate());
            userStatus.BindValueChanged(_ => schedulePresenceUpdate());
            userActivity.BindValueChanged(_ => schedulePresenceUpdate());
            privacyMode.BindValueChanged(_ => schedulePresenceUpdate());

            multiplayerClient.RoomUpdated += onRoomUpdated;
            statisticsProvider.StatisticsUpdated += onStatisticsUpdated;
        }

        protected override void Update()
        {
            base.Update();
            discord.RunCallbacks();
        }

        // todo: apparently not needed?
        // private void onReady(object _, ReadyMessage __)
        // {
        //     // when RPC is lost and reconnected, we have to clear presence state for updatePresence to work (see DiscordRpcClient.SkipIdenticalPresence).
        //     if (client.CurrentPresence != null)
        //         client.SetPresence(null);
        //
        //     schedulePresenceUpdate();
        // }

        private void onRoomUpdated() => schedulePresenceUpdate();

        private void onStatisticsUpdated(UserStatisticsUpdate _) => schedulePresenceUpdate();

        private ScheduledDelegate? presenceUpdateDelegate;

        private void schedulePresenceUpdate()
        {
            presenceUpdateDelegate?.Cancel();
            presenceUpdateDelegate = Scheduler.AddDelayed(() =>
            {
                // if (!client.IsInitialized)
                //     return;

                if (!api.IsLoggedIn || userStatus.Value == UserStatus.Offline || privacyMode.Value == DiscordRichPresenceMode.Off)
                {
                    activityManager.ClearActivity(_ => { });
                    return;
                }

                bool hideIdentifiableInformation = privacyMode.Value == DiscordRichPresenceMode.Limited || userStatus.Value == UserStatus.DoNotDisturb;

                updatePresence(hideIdentifiableInformation);
                activityManager.UpdateActivity(activity, _ => { });
            }, 200);
        }

        private void updatePresence(bool hideIdentifiableInformation)
        {
            if (user == null)
                return;

            // user activity
            if (userActivity.Value != null)
            {
                activity.State = clampLength(userActivity.Value.GetStatus(hideIdentifiableInformation));
                activity.Details = clampLength(userActivity.Value.GetDetails(hideIdentifiableInformation) ?? string.Empty);

                if (userActivity.Value.GetBeatmapID(hideIdentifiableInformation) is int beatmapId && beatmapId > 0)
                {
                    // todo: not a thing?????????
                    // activity.Buttons = new[]
                    // {
                    //     new Button
                    //     {
                    //         Label = "View beatmap",
                    //         Url = $@"{api.WebsiteRootUrl}/beatmaps/{beatmapId}?mode={ruleset.Value.ShortName}"
                    //     }
                    // };
                }
                else
                {
                    // activity.Buttons = null;
                }
            }
            else
            {
                activity.State = "Idle";
                activity.Details = string.Empty;
            }

            // user party
            if (!hideIdentifiableInformation && multiplayerClient.Room != null)
            {
                MultiplayerRoom room = multiplayerClient.Room;

                activity.Party = new ActivityParty
                {
                    Privacy = string.IsNullOrEmpty(room.Settings.Password) ? ActivityPartyPrivacy.Public : ActivityPartyPrivacy.Private,
                    Id = room.RoomID.ToString(),
                    Size = new PartySize
                    {
                        // technically lobbies can have infinite users, but Discord needs this to be set to something.
                        // to make party display sensible, assign a powers of two above participants count (8 at minimum).
                        MaxSize = (int)Math.Max(8, Math.Pow(2, Math.Ceiling(Math.Log2(room.Users.Count)))),
                        CurrentSize = room.Users.Count,
                    }
                };

                RoomSecret roomSecret = new RoomSecret
                {
                    RoomID = room.RoomID,
                    Password = room.Settings.Password,
                };

                // todo: not needed I think?
                // if (client.HasRegisteredUriScheme)
                activity.Secrets.Join = JsonConvert.SerializeObject(roomSecret);

                // discord cannot handle both secrets and buttons at the same time, so we need to choose something.
                // the multiplayer room seems more important.
                // activity.Buttons = null;
            }
            else
            {
                activity.Party = new ActivityParty();
                activity.Secrets.Join = string.Empty;
            }

            // game images:
            // large image tooltip
            if (privacyMode.Value == DiscordRichPresenceMode.Limited)
                activity.Assets.LargeText = string.Empty;
            else
            {
                var statistics = statisticsProvider.GetStatisticsFor(ruleset.Value);
                activity.Assets.LargeText = $"{user.Value.Username}" + (statistics?.GlobalRank > 0 ? $" (rank #{statistics.GlobalRank:N0})" : string.Empty);
            }

            // small image
            activity.Assets.SmallImage = ruleset.Value.IsLegacyRuleset() ? $"mode_{ruleset.Value.OnlineID}" : "mode_custom";
            activity.Assets.SmallText = ruleset.Value.Name;
        }

        private void onActivityJoin(string secret) => Scheduler.AddOnce(() =>
        {
            game.Window?.Raise();

            if (!api.IsLoggedIn)
            {
                login?.Show();
                return;
            }

            Logger.Log($"Received room secret from Discord RPC Client: \"{secret}\"", LoggingTarget.Network, LogLevel.Debug);

            // Stable and lazer share the same Discord client ID, meaning they can accept join requests from each other.
            // Since they aren't compatible in multi, see if stable's format is being used and log to avoid confusion.
            if (secret[0] != '{' || !tryParseRoomSecret(secret, out long roomId, out string? password))
            {
                Logger.Log("Could not join multiplayer room, invitation is invalid or incompatible.", LoggingTarget.Network, LogLevel.Important);
                return;
            }

            var request = new GetRoomRequest(roomId);
            request.Success += room => Schedule(() =>
            {
                game.PresentMultiplayerMatch(room, password);
            });
            request.Failure += _ => Logger.Log($"Could not join multiplayer room, room could not be found (room ID: {roomId}).", LoggingTarget.Network, LogLevel.Important);
            api.Queue(request);
        });

        private static readonly int ellipsis_length = Encoding.UTF8.GetByteCount(new[] { '…' });

        private static string clampLength(string str)
        {
            // Empty strings are fine to discord even though single-character strings are not. Make it make sense.
            if (string.IsNullOrEmpty(str))
                return str;

            // As above, discord decides that *non-empty* strings shorter than 2 characters cannot possibly be valid input, because... reasons?
            // And yes, that is two *characters*, or *codepoints*, not *bytes* as further down below (as determined by empirical testing).
            // Also, spaces don't count. Because reasons, clearly.
            // That all seems very questionable, and isn't even documented anywhere. So to *make it* accept such valid input,
            // just tack on enough of U+200B ZERO WIDTH SPACEs at the end. After making sure to trim whitespace.
            string trimmed = str.Trim();
            if (trimmed.Length < 2)
                return trimmed.PadRight(2, '\u200B');

            if (Encoding.UTF8.GetByteCount(str) <= 128)
                return str;

            ReadOnlyMemory<char> strMem = str.AsMemory();

            do
            {
                strMem = strMem[..^1];
            } while (Encoding.UTF8.GetByteCount(strMem.Span) + ellipsis_length > 128);

            return string.Create(strMem.Length + 1, strMem, (span, mem) =>
            {
                mem.Span.CopyTo(span);
                span[^1] = '…';
            });
        }

        private static bool tryParseRoomSecret(string secretJson, out long roomId, out string? password)
        {
            roomId = 0;
            password = null;

            RoomSecret? roomSecret;

            try
            {
                roomSecret = JsonConvert.DeserializeObject<RoomSecret>(secretJson);
            }
            catch
            {
                return false;
            }

            if (roomSecret == null) return false;

            roomId = roomSecret.RoomID;
            password = roomSecret.Password;

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (multiplayerClient.IsNotNull())
                multiplayerClient.RoomUpdated -= onRoomUpdated;

            if (statisticsProvider.IsNotNull())
                statisticsProvider.StatisticsUpdated -= onStatisticsUpdated;

            client.Dispose();
            base.Dispose(isDisposing);
        }

        private class RoomSecret
        {
            [JsonProperty(@"roomId", Required = Required.Always)]
            public long RoomID { get; set; }

            [JsonProperty(@"password", Required = Required.AllowNull)]
            public string? Password { get; set; }
        }
    }
}
