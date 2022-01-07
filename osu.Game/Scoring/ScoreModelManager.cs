// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;
using osu.Game.Stores;
using Realms;

#nullable enable

namespace osu.Game.Scoring
{
    public class ScoreModelManager : RealmArchiveModelManager<ScoreInfo>
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        private readonly RulesetStore rulesets;
        private readonly Func<BeatmapManager> beatmaps;

        public ScoreModelManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, RealmContextFactory contextFactory)
            : base(storage, contextFactory)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
        }

        protected override ScoreInfo? CreateModel(ArchiveReader archive)
        {
            using (var stream = archive.GetStream(archive.Filenames.First(f => f.EndsWith(".osr", StringComparison.OrdinalIgnoreCase))))
            {
                try
                {
                    return new DatabasedLegacyScoreDecoder(rulesets, beatmaps()).Parse(stream).ScoreInfo;
                }
                catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                {
                    Logger.Log(e.Message, LoggingTarget.Information, LogLevel.Error);
                    return null;
                }
            }
        }

        public Score GetScore(ScoreInfo score) => new LegacyDatabasedScore(score, rulesets, beatmaps(), Files.Store);

        protected override Task Populate(ScoreInfo model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            // Ensure the beatmap is not detached.
            if (!model.Beatmap.IsManaged)
                model.Beatmap = realm.Find<BeatmapInfo>(model.Beatmap.ID);

            if (!model.Ruleset.IsManaged)
                model.Ruleset = realm.Find<RulesetInfo>(model.Ruleset.ShortName);

            if (string.IsNullOrEmpty(model.StatisticsJson))
                model.StatisticsJson = JsonConvert.SerializeObject(model.Statistics);

            return Task.CompletedTask;
        }
    }
}
