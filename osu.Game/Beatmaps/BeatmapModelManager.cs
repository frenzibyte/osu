// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Stores;
using Realms;

#nullable enable

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles ef-core storage of beatmaps.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class BeatmapModelManager : BeatmapImporter
    {
        /// <summary>
        /// Fired when a single difficulty has been hidden.
        /// </summary>
        public event Action<BeatmapInfo>? BeatmapHidden;

        /// <summary>
        /// Fired when a single difficulty has been restored.
        /// </summary>
        public event Action<BeatmapInfo>? BeatmapRestored;

        /// <summary>
        /// The game working beatmap cache, used to invalidate entries on changes.
        /// </summary>
        public IWorkingBeatmapCache? WorkingBeatmapCache { private get; set; }

        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        private readonly RulesetStore rulesets;

        public BeatmapModelManager(Storage storage, RealmContextFactory contextFactory, RulesetStore rulesets, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(contextFactory, storage, onlineLookupQueue)
        {
            this.rulesets = rulesets;
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

        protected override BeatmapSetInfo? CreateModel(ArchiveReader archive)
        {
            return importer.CreateModel(archive);
        }

        protected override async Task Populate(BeatmapSetInfo beatmapSet, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken)
        {
            if (archive != null)
                beatmapSet.Beatmaps.AddRange(createBeatmapDifficulties(beatmapSet.Files));

            foreach (BeatmapInfo b in beatmapSet.Beatmaps)
            {
                // remove metadata from difficulties where it matches the set
                if (beatmapSet.Metadata.Equals(b.Metadata))
                    b.Metadata = null;

                b.BeatmapSet = beatmapSet;
            }

            validateOnlineIds(beatmapSet);

            bool hadOnlineIDs = beatmapSet.Beatmaps.Any(b => b.OnlineID > 0);

            if (OnlineLookupQueue != null)
                await OnlineLookupQueue.UpdateAsync(beatmapSet, cancellationToken).ConfigureAwait(false);

            // ensure at least one beatmap was able to retrieve or keep an online ID, else drop the set ID.
            if (hadOnlineIDs && !beatmapSet.Beatmaps.Any(b => b.OnlineID > 0))
            {
                if (beatmapSet.OnlineID != null)
                {
                    beatmapSet.OnlineID = -1;
                    LogForModel(beatmapSet, "Disassociating beatmap set ID due to loss of all beatmap IDs");
                }
            }
        }

        /// <summary>
        /// Delete a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to hide.</param>
        public void Hide(BeatmapInfo beatmapInfo) => beatmaps.Hide(beatmapInfo);

        /// <summary>
        /// Restore a beatmap difficulty.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap difficulty to restore.</param>
        public void Restore(BeatmapInfo beatmapInfo) => beatmaps.Restore(beatmapInfo);

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo beatmapInfo, IBeatmap beatmapContent, ISkin beatmapSkin = null)
        {
            var setInfo = beatmapInfo.BeatmapSet;

            // Difficulty settings must be copied first due to the clone in `Beatmap<>.BeatmapInfo_Set`.
            // This should hopefully be temporary, assuming said clone is eventually removed.

            // Warning: The directionality here is important. Changes have to be copied *from* beatmapContent (which comes from editor and is being saved)
            // *to* the beatmapInfo (which is a database model and needs to receive values without the taiko slider velocity multiplier for correct operation).
            // CopyTo() will undo such adjustments, while CopyFrom() will not.
            beatmapContent.Difficulty.CopyTo(beatmapInfo.BaseDifficulty);

            // All changes to metadata are made in the provided beatmapInfo, so this should be copied to the `IBeatmap` before encoding.
            beatmapContent.BeatmapInfo = beatmapInfo;

            using (var stream = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                    new LegacyBeatmapEncoder(beatmapContent, beatmapSkin).Encode(sw);

                stream.Seek(0, SeekOrigin.Begin);

                using (ContextFactory.GetForWrite())
                {
                    beatmapInfo = setInfo.Beatmaps.Single(b => b.Equals(beatmapInfo));

                    var metadata = beatmapInfo.Metadata ?? setInfo.Metadata;

                    // grab the original file (or create a new one if not found).
                    var fileInfo = setInfo.Files.SingleOrDefault(f => string.Equals(f.Filename, beatmapInfo.Path, StringComparison.OrdinalIgnoreCase)) ?? new BeatmapSetFileInfo();

                    // metadata may have changed; update the path with the standard format.
                    beatmapInfo.Path = $"{metadata.Artist} - {metadata.Title} ({metadata.Author}) [{beatmapInfo.DifficultyName}].osu".GetValidArchiveContentFilename();

                    beatmapInfo.MD5Hash = stream.ComputeMD5Hash();

                    // update existing or populate new file's filename.
                    fileInfo.Filename = beatmapInfo.Path;

                    stream.Seek(0, SeekOrigin.Begin);
                    ReplaceFile(setInfo, fileInfo, stream);
                }
            }

            WorkingBeatmapCache?.Invalidate(beatmapInfo);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapSetInfo QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query) => beatmaps.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override bool CanSkipImport(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanSkipImport(existing, import))
                return false;

            return existing.Beatmaps.Any(b => b.OnlineID != null);
        }

        protected override bool CanReuseExisting(BeatmapSetInfo existing, BeatmapSetInfo import)
        {
            if (!base.CanReuseExisting(existing, import))
                return false;

            var existingIds = existing.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);
            var importIds = import.Beatmaps.Select(b => b.OnlineID).OrderBy(i => i);

            // force re-import if we are not in a sane state.
            return existing.OnlineID == import.OnlineID && existingIds.SequenceEqual(importIds);
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets(IncludedDetails includes = IncludedDetails.All, bool includeProtected = false) =>
            GetAllUsableBeatmapSetsEnumerable(includes, includeProtected).ToList();

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s. Note that files are not populated.
        /// </summary>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <param name="includeProtected">Whether to include protected (system) beatmaps. These should not be included for gameplay playable use cases.</param>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public IEnumerable<BeatmapSetInfo> GetAllUsableBeatmapSetsEnumerable(IncludedDetails includes, bool includeProtected = false)
        {
            IQueryable<BeatmapSetInfo> queryable;

            switch (includes)
            {
                case IncludedDetails.Minimal:
                    queryable = beatmaps.BeatmapSetsOverview;
                    break;

                case IncludedDetails.AllButRuleset:
                    queryable = beatmaps.BeatmapSetsWithoutRuleset;
                    break;

                case IncludedDetails.AllButFiles:
                    queryable = beatmaps.BeatmapSetsWithoutFiles;
                    break;

                default:
                    queryable = beatmaps.ConsumableItems;
                    break;
            }

            // AsEnumerable used here to avoid applying the WHERE in sql. When done so, ef core 2.x uses an incorrect ORDER BY
            // clause which causes queries to take 5-10x longer.
            // TODO: remove if upgrading to EF core 3.x.
            return queryable.AsEnumerable().Where(s => !s.DeletePending && (includeProtected || !s.Protected));
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="includes">The level of detail to include in the returned objects.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query, IncludedDetails includes = IncludedDetails.All)
        {
            IQueryable<BeatmapSetInfo> queryable;

            switch (includes)
            {
                case IncludedDetails.Minimal:
                    queryable = beatmaps.BeatmapSetsOverview;
                    break;

                case IncludedDetails.AllButRuleset:
                    queryable = beatmaps.BeatmapSetsWithoutRuleset;
                    break;

                case IncludedDetails.AllButFiles:
                    queryable = beatmaps.BeatmapSetsWithoutFiles;
                    break;

                default:
                    queryable = beatmaps.ConsumableItems;
                    break;
            }

            return queryable.AsNoTracking().Where(query);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().FirstOrDefault(query);

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query) => beatmaps.Beatmaps.AsNoTracking().Where(query);
    }

    /// <summary>
    /// The level of detail to include in database results.
    /// </summary>
    public enum IncludedDetails
    {
        /// <summary>
        /// Only include beatmap difficulties and set level metadata.
        /// </summary>
        Minimal,

        /// <summary>
        /// Include all difficulties, rulesets, difficulty metadata but no files.
        /// </summary>
        AllButFiles,

        /// <summary>
        /// Include everything except ruleset. Used for cases where we aren't sure the ruleset is present but still want to consume the beatmap.
        /// </summary>
        AllButRuleset,

        /// <summary>
        /// Include everything.
        /// </summary>
        All
    }
}
