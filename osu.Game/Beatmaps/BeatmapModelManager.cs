// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Skinning;
using osu.Game.Stores;

#nullable enable

namespace osu.Game.Beatmaps
{
    [ExcludeFromDynamicCompile]
    public class BeatmapModelManager : BeatmapImporter
    {
        /// <summary>
        /// The game working beatmap cache, used to invalidate entries on changes.
        /// </summary>
        public IWorkingBeatmapCache? WorkingBeatmapCache { private get; set; }

        public override IEnumerable<string> HandledExtensions => new[] { ".osz" };

        protected override string[] HashableFileTypes => new[] { ".osu" };

        public BeatmapModelManager(RealmContextFactory contextFactory, Storage storage, BeatmapOnlineLookupQueue? onlineLookupQueue = null)
            : base(contextFactory, storage, onlineLookupQueue)
        {
        }

        protected override bool ShouldDeleteArchive(string path) => Path.GetExtension(path)?.ToLowerInvariant() == ".osz";

        /// <summary>
        /// Saves an <see cref="IBeatmap"/> file against a given <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The <see cref="BeatmapInfo"/> to save the content against. The file referenced by <see cref="BeatmapInfo.Path"/> will be replaced.</param>
        /// <param name="beatmapContent">The <see cref="IBeatmap"/> content to write.</param>
        /// <param name="beatmapSkin">The beatmap <see cref="ISkin"/> content to write, null if to be omitted.</param>
        public virtual void Save(BeatmapInfo beatmapInfo, IBeatmap beatmapContent, ISkin? beatmapSkin = null)
        {
            var setInfo = beatmapInfo.BeatmapSet;

            Debug.Assert(setInfo != null);

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

                using (var realm = ContextFactory.CreateContext())
                using (var transaction = realm.BeginWrite())
                {
                    beatmapInfo = setInfo.Beatmaps.Single(b => b.Equals(beatmapInfo));

                    // grab the original file (or create a new one if not found).
                    var existingFileInfo = setInfo.Files.SingleOrDefault(f => string.Equals(f.Filename, beatmapInfo.Path, StringComparison.OrdinalIgnoreCase));

                    if (existingFileInfo != null)
                    {
                        DeleteFile(setInfo, existingFileInfo);
                    }

                    // metadata may have changed; update the path with the standard format.
                    var metadata = beatmapInfo.Metadata;
                    string filename = $"{metadata.Artist} - {metadata.Title} ({metadata.Author}) [{beatmapInfo.DifficultyName}].osu".GetValidArchiveContentFilename();

                    beatmapInfo.MD5Hash = stream.ComputeMD5Hash();

                    stream.Seek(0, SeekOrigin.Begin);
                    AddFile(setInfo, stream, filename, realm);

                    transaction.Commit();
                }
            }

            WorkingBeatmapCache?.Invalidate(beatmapInfo);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public ILive<BeatmapSetInfo>? QueryBeatmapSet(Expression<Func<BeatmapSetInfo, bool>> query)
        {
            using (var context = ContextFactory.CreateContext())
                return context.All<BeatmapSetInfo>().FirstOrDefault(query)?.ToLive();
        }

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public List<BeatmapSetInfo> GetAllUsableBeatmapSets(bool includeProtected = false) =>
            GetAllUsableBeatmapSetsEnumerable(includeProtected).ToList();

        /// <summary>
        /// Returns a list of all usable <see cref="BeatmapSetInfo"/>s. Note that files are not populated.
        /// </summary>
        /// <param name="includeProtected">Whether to include protected (system) beatmaps. These should not be included for gameplay playable use cases.</param>
        /// <returns>A list of available <see cref="BeatmapSetInfo"/>.</returns>
        public IEnumerable<BeatmapSetInfo> GetAllUsableBeatmapSetsEnumerable(bool includeProtected = false)
        {
            using (var context = ContextFactory.CreateContext())
                return context.All<BeatmapSetInfo>().Where(b => !b.DeletePending && (includeProtected || !b.Protected));
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapSetInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<BeatmapSetInfo> QueryBeatmapSets(Expression<Func<BeatmapSetInfo, bool>> query)
        {
            using (var context = ContextFactory.CreateContext())
            {
                return context.All<BeatmapSetInfo>()
                              .Where(b => !b.DeletePending)
                              .Where(query);
            }
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public BeatmapInfo? QueryBeatmap(Expression<Func<BeatmapInfo, bool>> query)
        {
            using (var context = ContextFactory.CreateContext())
                return context.All<BeatmapInfo>().FirstOrDefault(query); // TODO: ?.ToLive();
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="BeatmapInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IQueryable<BeatmapInfo> QueryBeatmaps(Expression<Func<BeatmapInfo, bool>> query)
        {
            using (var context = ContextFactory.CreateContext())
            {
                return context.All<BeatmapInfo>()
                              .Where(query);
            }
        }
    }
}
