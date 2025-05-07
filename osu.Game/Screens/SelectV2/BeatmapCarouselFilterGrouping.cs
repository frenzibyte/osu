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

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterGrouping : ICarouselFilter
    {
        public bool BeatmapSetsGroupedTogether { get; private set; }

        /// <summary>
        /// Beatmap sets contain difficulties as related panels. This dictionary holds the relationships between set-difficulties to allow expanding them on selection.
        /// </summary>
        public IDictionary<BeatmapSetInfo, HashSet<CarouselItem>> SetItems => setItems;

        /// <summary>
        /// Groups contain children which are group-selectable. This dictionary holds the relationships between groups-panels to allow expanding them on selection.
        /// </summary>
        public IDictionary<GroupDefinition, HashSet<CarouselItem>> GroupItems => groupItems;

        private readonly Dictionary<BeatmapSetInfo, HashSet<CarouselItem>> setItems = new Dictionary<BeatmapSetInfo, HashSet<CarouselItem>>();
        private readonly Dictionary<GroupDefinition, HashSet<CarouselItem>> groupItems = new Dictionary<GroupDefinition, HashSet<CarouselItem>>();

        private readonly Func<FilterCriteria> getCriteria;

        public BeatmapCarouselFilterGrouping(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                setItems.Clear();
                groupItems.Clear();

                var criteria = getCriteria();
                var newItems = new List<CarouselItem>();

                BeatmapInfo? lastBeatmap = null;

                GroupDefinition? lastGroup = null;
                CarouselItem? lastGroupItem = null;

                HashSet<CarouselItem>? currentGroupItems = null;
                HashSet<CarouselItem>? currentSetItems = null;

                BeatmapSetsGroupedTogether = criteria.Group != GroupMode.Difficulty;

                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var beatmap = (BeatmapInfo)item.Model;

                    if (createGroupIfRequired(criteria, beatmap, lastGroup) is GroupDefinition newGroup)
                    {
                        // When reaching a new group, ensure we reset any beatmap set tracking.
                        currentSetItems = null;
                        lastBeatmap = null;

                        groupItems[newGroup] = currentGroupItems = new HashSet<CarouselItem>();
                        lastGroup = newGroup;

                        addItem(lastGroupItem = new CarouselItem(newGroup)
                        {
                            DrawHeight = PanelGroup.HEIGHT,
                            DepthLayer = -2,
                        });
                    }

                    if (BeatmapSetsGroupedTogether)
                    {
                        bool newBeatmapSet = lastBeatmap?.BeatmapSet!.ID != beatmap.BeatmapSet!.ID;

                        if (newBeatmapSet)
                        {
                            setItems[beatmap.BeatmapSet!] = currentSetItems = new HashSet<CarouselItem>();

                            if (lastGroupItem != null)
                                lastGroupItem.NestedItemCount++;

                            addItem(new CarouselItem(beatmap.BeatmapSet!)
                            {
                                DrawHeight = PanelBeatmapSet.HEIGHT,
                                DepthLayer = -1
                            });
                        }
                    }
                    else
                    {
                        if (lastGroupItem != null)
                            lastGroupItem.NestedItemCount++;
                    }

                    addItem(item);
                    lastBeatmap = beatmap;

                    void addItem(CarouselItem i)
                    {
                        newItems.Add(i);

                        currentGroupItems?.Add(i);
                        currentSetItems?.Add(i);

                        i.IsVisible = i.Model is GroupDefinition || (lastGroup == null && (i.Model is BeatmapSetInfo || currentSetItems == null));
                    }
                }

                return newItems;
            }, cancellationToken).ConfigureAwait(false);
        }

        private GroupDefinition? createGroupIfRequired(FilterCriteria criteria, BeatmapInfo beatmap, GroupDefinition? lastGroup)
        {
            switch (criteria.Group)
            {
                case GroupMode.NoGrouping:
                    return null;

                case GroupMode.Artist:
                    return groupFromFirstLetter(beatmap.Metadata.Artist, lastGroup);

                case GroupMode.Author:
                    return groupFromFirstLetter(beatmap.Metadata.Author.Username, lastGroup);

                case GroupMode.Title:
                    return groupFromFirstLetter(beatmap.Metadata.Title, lastGroup);

                case GroupMode.BPM:
                    break;

                case GroupMode.Collections:
                    break;

                case GroupMode.DateAdded:
                    break;

                case GroupMode.Favourites:
                    break;

                case GroupMode.Length:
                    break;

                case GroupMode.MyMaps:
                    break;

                case GroupMode.RankAchieved:
                    break;

                case GroupMode.RankedStatus:
                    break;

                case GroupMode.RecentlyPlayed:
                    break;

                case GroupMode.Difficulty:
                    int starGroup = lastGroup?.Data as int? ?? -1;

                    if (beatmap.StarRating > starGroup)
                    {
                        starGroup = (int)Math.Floor(beatmap.StarRating);
                        return new GroupDefinition(starGroup + 1, $"{starGroup} - {starGroup + 1} *");
                    }

                    break;
            }

            return null;
        }

        private GroupDefinition? groupFromFirstLetter(string name, GroupDefinition? lastGroup)
        {
            char lastFirstChar = lastGroup?.Data as char? ?? (char)0;
            char firstChar = char.ToUpperInvariant(name[0]);

            if (firstChar > lastFirstChar)
                return new GroupDefinition(firstChar, $"{firstChar}");

            return null;
        }
    }
}
