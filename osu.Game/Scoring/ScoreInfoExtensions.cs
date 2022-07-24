// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public static class ScoreInfoExtensions
    {
        /// <summary>
        /// Retrieves the ruleset's short name for the provided <see cref="IScoreInfo"/>.
        /// </summary>
        // TODO: this should really not exist.
        public static string GetRulesetShortName(this IScoreInfo score)
        {
            if (score is ScoreInfo localScore)
                return localScore.Ruleset.ShortName;

            switch (score.RulesetID)
            {
                case 0:
                    return "osu";

                case 1:
                    return "taiko";

                case 2:
                    return "fruits";

                case 3:
                    return "mania";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Retrieves a local <see cref="RulesetInfo"/> for the provided <see cref="IScoreInfo"/>.
        /// </summary>
        public static RulesetInfo GetRuleset(this IScoreInfo score, RulesetStore rulesets)
        {
            if (score is ScoreInfo localScore)
                return localScore.Ruleset;

            return rulesets.GetRuleset(score.RulesetID) ?? throw new InvalidOperationException($"Ruleset with ID of {score.RulesetID} could not be found locally.");
        }

        /// <summary>
        /// Retrieves instances of the selected mods for the provided <see cref="IScoreInfo"/>.
        /// </summary>
        public static Mod[] GetMods(this IScoreInfo score, RulesetStore rulesets)
        {
            throw new NotImplementedException();
        }

        public static bool IsLegacyScore(this IScoreInfo score) => score.Mods.Any(m => m.Acronym == "CL");

        public static IEnumerable<HitResultDisplayStatistic> GetStatisticsForDisplay(this IScoreInfo score, RulesetStore rulesets)
        {
            foreach (var r in score.GetRuleset(rulesets).CreateInstance().GetHitResults())
            {
                int value = score.Statistics.GetValueOrDefault(r.result);

                switch (r.result)
                {
                    case HitResult.SmallTickHit:
                    {
                        int total = value + score.Statistics.GetValueOrDefault(HitResult.SmallTickMiss);
                        if (total > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, total, r.displayName);

                        break;
                    }

                    case HitResult.LargeTickHit:
                    {
                        int total = value + score.Statistics.GetValueOrDefault(HitResult.LargeTickMiss);
                        if (total > 0)
                            yield return new HitResultDisplayStatistic(r.result, value, total, r.displayName);

                        break;
                    }

                    case HitResult.SmallTickMiss:
                    case HitResult.LargeTickMiss:
                        break;

                    default:
                        yield return new HitResultDisplayStatistic(r.result, value, null, r.displayName);

                        break;
                }
            }
        }

        /// <summary>
        /// A user-presentable display title representing this score.
        /// </summary>
        public static string GetDisplayTitle(this IScoreInfo score) => $"{score.User.Username} playing {score.Beatmap.GetDisplayTitle()}";
    }
}
