// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Utils;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterSorting : ICarouselFilter
    {
        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterSorting(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            var criteria = getCriteria();

            return items.Order(Comparer<CarouselItem>.Create((a, b) =>
            {
                int comparison;

                var ab = (BeatmapInfo)a.Model;
                var bb = (BeatmapInfo)b.Model;

                // TODO: beatmaps with variable metadata are gonna play funnily here.
                switch (criteria.Sort)
                {
                    case SortMode.Artist:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Artist, bb.Metadata.Artist);
                        if (comparison == 0)
                            goto case SortMode.Title;
                        break;

                    case SortMode.Title:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Title, bb.Metadata.Title);
                        break;

                    case SortMode.Author:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Author.Username, bb.Metadata.Author.Username);
                        break;

                    case SortMode.Source:
                        comparison = OrdinalSortByCaseStringComparer.DEFAULT.Compare(ab.Metadata.Source, bb.Metadata.Source);
                        break;

                    case SortMode.Difficulty:
                        comparison = ab.StarRating.CompareTo(bb.StarRating);
                        break;

                    case SortMode.DateAdded:
                        comparison = bb.BeatmapSet!.DateAdded.CompareTo(ab.BeatmapSet!.DateAdded);
                        break;

                    case SortMode.DateRanked:
                        comparison = Nullable.Compare(bb.BeatmapSet!.DateRanked, ab.BeatmapSet!.DateRanked);
                        break;

                    case SortMode.DateSubmitted:
                        comparison = Nullable.Compare(bb.BeatmapSet!.DateSubmitted, ab.BeatmapSet!.DateSubmitted);
                        break;

                    case SortMode.LastPlayed:
                        comparison = -compareUsingAggregateMax(ab, bb, items, static b => (b.LastPlayed ?? DateTimeOffset.MinValue).ToUnixTimeSeconds());
                        break;

                    case SortMode.BPM:
                        comparison = compareUsingAggregateMax(ab, bb, items, static b => b.BPM);
                        break;

                    case SortMode.Length:
                        comparison = compareUsingAggregateMax(ab, bb, items, static b => b.Length);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return comparison;
            })).ToList();
        }, cancellationToken).ConfigureAwait(false);

        private int compareUsingAggregateMax(BeatmapInfo a, BeatmapInfo b, IEnumerable<CarouselItem> items, Func<BeatmapInfo, double> func)
        {
            var aBeatmaps = items.Select(i => i.Model).Cast<BeatmapInfo>().Where(beatmap => beatmap.BeatmapSet!.Equals(a.BeatmapSet));
            var bBeatmaps = items.Select(i => i.Model).Cast<BeatmapInfo>().Where(beatmap => beatmap.BeatmapSet!.Equals(b.BeatmapSet));

            bool aAny = aBeatmaps.Any();
            bool bAny = bBeatmaps.Any();

            if (!aAny && !bAny) return 0;
            if (!aAny) return -1;
            if (!bAny) return 1;

            return aBeatmaps.Max(func).CompareTo(bBeatmaps.Max(func));
        }
    }
}
