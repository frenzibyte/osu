// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        public bool BeatmapSetsGroupedTogether { get; private set; }

        /// <summary>
        /// All carousel items currently displayable by the carousel.
        /// </summary>
        public IEnumerable<CarouselItem> AllItems => allItems ?? Enumerable.Empty<CarouselItem>();

        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setMap;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupMap;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setMap = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();
        private readonly Dictionary<GroupDefinition, HashSet<CarouselItem>> groupMap = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();
        private List<CarouselItem>? allItems;

        private readonly Func<FilterCriteria> getCriteria;
        private readonly Func<List<Live<BeatmapCollection>>> getCollections;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria, Func<List<Live<BeatmapCollection>>> getCollections)
        {
            this.getCriteria = getCriteria;
            this.getCollections = getCollections;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                setMap.Clear();
                groupMap.Clear();

                var criteria = getCriteria();
                var newItems = new List<CarouselItem>();

                BeatmapSetsGroupedTogether = criteria.Group != GroupMode.Difficulty;

                var groups = getGroups((List<CarouselItem>)items, criteria);

                foreach (var (group, itemsInGroup) in groups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    CarouselItem? groupItem = null;
                    HashSet<CarouselItem>? currentGroupItems = null;
                    HashSet<CarouselItem>? currentSetItems = null;
                    BeatmapInfo? lastBeatmap = null;

                    if (group != null)
                    {
                        groupMap[group] = currentGroupItems = new HashSet<CarouselItem>();

                        addItem(groupItem = new CarouselItem(group)
                        {
                            DrawHeight = PanelGroup.HEIGHT,
                            DepthLayer = -2,
                        });
                    }

                    foreach (var item in itemsInGroup)
                    {
                        var beatmap = (BeatmapInfo)item.Model;

                        if (BeatmapSetsGroupedTogether)
                        {
                            bool newBeatmapSet = lastBeatmap?.BeatmapSet!.ID != beatmap.BeatmapSet!.ID;

                            if (newBeatmapSet)
                            {
                                if (!setMap.TryGetValue(beatmap.BeatmapSet!, out currentSetItems))
                                    setMap[beatmap.BeatmapSet!] = currentSetItems = new HashSet<CarouselItem>();

                                if (groupItem != null)
                                    groupItem.NestedItemCount++;

                                addItem(new CarouselItem(beatmap.BeatmapSet!)
                                {
                                    DrawHeight = PanelBeatmapSet.HEIGHT,
                                    DepthLayer = -1
                                });
                            }
                        }
                        else
                        {
                            if (groupItem != null)
                                groupItem.NestedItemCount++;
                        }

                        addItem(item);
                        lastBeatmap = beatmap;
                    }

                    void addItem(CarouselItem i)
                    {
                        newItems.Add(i);

                        currentGroupItems?.Add(i);
                        currentSetItems?.Add(i);

                        i.IsVisible = i.Model is GroupDefinition || (group == null && (i.Model is BeatmapSetInfo || currentSetItems == null));
                    }
                }

                allItems = newItems;
                return newItems;
            }, cancellationToken).ConfigureAwait(false);
        }

        // todo: there can still be more than one beatmap set panel, we still need to fix behaviour when that's the case.
        // todo: is this localisable?
        private List<(GroupDefinition?, List<CarouselItem>)> getGroups(List<CarouselItem> items, FilterCriteria criteria)
        {
            switch (criteria.Group)
            {
                case GroupMode.NoGrouping:
                    return new List<(GroupDefinition?, List<CarouselItem>)> { (null, items) };

                case GroupMode.Artist:
                    return getGroupsBy(b => defineGroupAlphabetically(b.Metadata.Artist), items);

                case GroupMode.Author:
                    return getGroupsBy(b => defineGroupAlphabetically(b.Metadata.Author.Username), items);

                case GroupMode.Title:
                    return getGroupsBy(b => defineGroupAlphabetically(b.Metadata.Title), items);

                case GroupMode.DateAdded:
                    return getGroupsBy(b => defineGroupByDate(b.BeatmapSet!.DateAdded), items);

                case GroupMode.RecentlyPlayed:
                    return getGroupsBy(b =>
                    {
                        if (b.LastPlayed == null)
                            return new GroupDefinition(int.MaxValue, "Never");

                        return defineGroupByDate(b.LastPlayed.Value);
                    }, items);

                case GroupMode.RankedStatus:
                    return getGroupsBy(b => defineGroupByStatus(b.BeatmapSet!.Status), items);

                case GroupMode.BPM:
                    return getGroupsBy(b => defineGroupsByBPM(b.BPM), items);

                case GroupMode.Difficulty:
                    return getGroupsBy(b => defineGroupsByStars(b.StarRating), items);

                case GroupMode.Length:
                    return getGroupsBy(b => defineGroupsByLength(b.Length), items);

                case GroupMode.Collections:
                    return getGroupsBy(b => defineGroupsByCollections(b.MD5Hash), items);

                case GroupMode.Favourites:
                    // todo: unsupported.
                    goto case GroupMode.NoGrouping;

                case GroupMode.MyMaps:
                    // todo: unsupported.
                    goto case GroupMode.NoGrouping;

                case GroupMode.RankAchieved:
                    // todo: unsupported.
                    goto case GroupMode.NoGrouping;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<(GroupDefinition?, List<CarouselItem>)> getGroupsBy(Func<BeatmapInfo, GroupDefinition?> getGroup, List<CarouselItem> items)
        {
            return items.GroupBy(i => getGroup((BeatmapInfo)i.Model))
                        .Where(g => g.Key != null)
                        .OrderBy(s => s.Key!.Data)
                        .Select(g => (g.Key, g.ToList()))
                        .ToList();
        }

        private GroupDefinition defineGroupAlphabetically(string name)
        {
            char firstChar = name.FirstOrDefault();

            if (char.IsAsciiDigit(firstChar))
                return new GroupDefinition(int.MinValue, "0-9");

            if (char.IsAsciiLetter(firstChar))
                return new GroupDefinition(char.ToUpperInvariant(firstChar) - 'A', char.ToUpperInvariant(firstChar).ToString());

            return new GroupDefinition(int.MaxValue, "Other");
        }

        private GroupDefinition defineGroupByDate(DateTimeOffset date)
        {
            var now = DateTimeOffset.Now;
            var elapsed = now - date;

            if (elapsed.TotalDays < 1)
                return new GroupDefinition(1, "Today");

            if (elapsed.TotalDays < 2)
                return new GroupDefinition(2, "Yesterday");

            if (elapsed.TotalDays < 7)
                return new GroupDefinition(7, "Last week");

            if (elapsed.TotalDays < 30)
                return new GroupDefinition(30, "1 month ago");

            for (int i = 60; i <= 150; i += 30)
            {
                if (elapsed.TotalDays < i)
                    return new GroupDefinition(i, $"{i / 30} months ago");
            }

            return new GroupDefinition(151, "Over 5 months ago");
        }

        private GroupDefinition defineGroupByStatus(BeatmapOnlineStatus status)
        {
            int order;

            if (status == BeatmapOnlineStatus.Approved)
                status = BeatmapOnlineStatus.Ranked;

            switch (status)
            {
                case BeatmapOnlineStatus.Ranked:
                    order = 0;
                    break;

                case BeatmapOnlineStatus.Qualified:
                    order = 1;
                    break;

                case BeatmapOnlineStatus.WIP:
                    order = 2;
                    break;

                case BeatmapOnlineStatus.Pending:
                    order = 3;
                    break;

                case BeatmapOnlineStatus.Graveyard:
                    order = 4;
                    break;

                case BeatmapOnlineStatus.LocallyModified:
                    order = 5;
                    break;

                case BeatmapOnlineStatus.None:
                    order = 6;
                    break;

                case BeatmapOnlineStatus.Loved:
                    order = 7;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            return new GroupDefinition(order, status.GetDescription());
        }

        private GroupDefinition defineGroupsByBPM(double bpm)
        {
            for (int i = 1; i < 6; i++)
            {
                if (bpm < i * 60)
                    return new GroupDefinition(i, $"Under {i * 60} BPM");
            }

            return new GroupDefinition(6, "Over 300 BPM");
        }

        private GroupDefinition defineGroupsByStars(double stars)
        {
            int starInt = (int)Math.Floor(stars);
            if (starInt == 0)
                return new GroupDefinition(0, "Below 1 Star");

            if (starInt == 1)
                return new GroupDefinition(1, "1 Star");

            return new GroupDefinition(starInt, $"{starInt} Stars");
        }

        private GroupDefinition defineGroupsByLength(double length)
        {
            for (int i = 1; i < 6; i++)
            {
                if (length <= i * 60_000)
                {
                    if (i == 1)
                        return new GroupDefinition(1, "1 minute or less");

                    return new GroupDefinition(i, $"{i} minutes or less");
                }
            }

            if (length <= 10 * 60_000)
                return new GroupDefinition(10, "10 minutes or less");

            return new GroupDefinition(11, "Over 10 minutes");
        }

        private GroupDefinition? defineGroupsByCollections(string md5Hash)
        {
            foreach (var collection in getCollections())
            {
                string? result = collection.PerformRead(c =>
                {
                    if (c.BeatmapMD5Hashes.Contains(md5Hash))
                        return c.Name;

                    return null;
                });

                if (result != null)
                    return new GroupDefinition((int)result[0], result);
            }

            // discard beatmaps with no collection from carousel, similar to stable.
            return null;
        }
    }
}
