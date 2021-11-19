﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using Realms;

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A single beatmap difficulty.
    /// </summary>
    [ExcludeFromDynamicCompile]
    [Serializable]
    [MapTo("Beatmap")]
    public class BeatmapInfo : RealmObject, IHasGuidPrimaryKey, IBeatmapInfo, IEquatable<BeatmapInfo>
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string DifficultyName { get; set; } = string.Empty;

        public RulesetInfo Ruleset { get; set; } = null!;

        public BeatmapDifficulty Difficulty { get; set; } = null!;

        public BeatmapMetadata Metadata { get; set; } = null!;

        public BeatmapSetInfo? BeatmapSet { get; set; }

        [Ignored]
        public RealmNamedFileUsage? File => BeatmapSet?.Files.First(f => f.File.Hash == Hash);

        public BeatmapOnlineStatus Status
        {
            get => (BeatmapOnlineStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        [MapTo(nameof(Status))]
        public int StatusInt { get; set; } = (int)BeatmapOnlineStatus.None;

        [Indexed]
        public int OnlineID { get; set; } = -1;

        public double Length { get; set; }

        public double BPM { get; set; }

        public string Hash { get; set; } = string.Empty;

        public double StarRating { get; set; }

        public string MD5Hash { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Hidden { get; set; }

        public BeatmapInfo(RulesetInfo ruleset, BeatmapDifficulty difficulty, BeatmapMetadata metadata)
        {
            Ruleset = ruleset;
            Difficulty = difficulty;
            Metadata = metadata;
        }

        [UsedImplicitly]
        public BeatmapInfo()
        {
        }

        #region Properties we may not want persisted (but also maybe no harm?)

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; }

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; }

        public double DistanceSpacing { get; set; }

        public int BeatDivisor { get; set; }

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; }

        public CountdownType Countdown { get; set; } = CountdownType.Normal;

        /// <summary>
        /// The number of beats to move the countdown backwards (compared to its default location).
        /// </summary>
        public int CountdownOffset { get; set; }

        #endregion

        public bool Equals(BeatmapInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return ID == other.ID;
        }

        public bool Equals(IBeatmapInfo? other) => other is BeatmapInfo b && Equals(b);

        public bool AudioEquals(BeatmapInfo? other) => other != null
                                                       && BeatmapSet != null
                                                       && other.BeatmapSet != null
                                                       && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                       && Metadata.AudioFile == other.Metadata.AudioFile;

        public bool BackgroundEquals(BeatmapInfo? other) => other != null
                                                            && BeatmapSet != null
                                                            && other.BeatmapSet != null
                                                            && BeatmapSet.Hash == other.BeatmapSet.Hash
                                                            && Metadata.BackgroundFile == other.Metadata.BackgroundFile;

        IBeatmapMetadataInfo IBeatmapInfo.Metadata => Metadata;
        IBeatmapSetInfo? IBeatmapInfo.BeatmapSet => BeatmapSet;
        IRulesetInfo IBeatmapInfo.Ruleset => Ruleset;
        IBeatmapDifficultyInfo IBeatmapInfo.Difficulty => Difficulty;

        #region Compatibility properties

        [Ignored]
        public int RulesetID => Ruleset.OnlineID;

        [Ignored]
        public Guid BeatmapSetInfoID => BeatmapSet?.ID ?? Guid.Empty;

        [Ignored]
        public BeatmapDifficulty BaseDifficulty
        {
            get => Difficulty;
            set => Difficulty = value;
        }

        [Ignored]
        public string? Path => File?.Filename;

        [Ignored]
        public APIBeatmap? OnlineInfo { get; set; }

        [Ignored]
        public int? MaxCombo { get; set; }

        [Ignored]
        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public int BeatmapVersion;

        public BeatmapInfo Clone() => this.Detach();

        #endregion
    }
}
