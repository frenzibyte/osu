// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSetV2.Header;
using osu.Game.Overlays.BeatmapSetV2.Info;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2
{
    public class BeatmapInfoOverlay : OnlineOverlay<BeatmapInfoHeader>
    {
        [Cached(typeof(IBindable<APIBeatmapSet?>))]
        private readonly Bindable<APIBeatmapSet?> beatmapSet = new Bindable<APIBeatmapSet?>();

        [Cached]
        [Cached(typeof(IBindable<IRulesetInfo?>))]
        private readonly Bindable<IRulesetInfo?> ruleset = new Bindable<IRulesetInfo?>();

        [Cached]
        [Cached(typeof(IBindable<APIBeatmap?>))]
        private readonly Bindable<APIBeatmap?> beatmap = new Bindable<APIBeatmap?>();

        [Cached]
        [Cached(typeof(IBindable<IReadOnlyList<APIBeatmap>>))]
        private readonly Bindable<IReadOnlyList<APIBeatmap>> availableBeatmaps = new Bindable<IReadOnlyList<APIBeatmap>>();

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        // todo: rethink-able
        protected override Color4 BackgroundColour => base.BackgroundColour.Darken(0.25f);

        public BeatmapInfoOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding { Bottom = 30 },
                        Children = new Drawable[]
                        {
                            new BeatmapInfoCover(),
                            new BeatmapInfoSubheader(),
                            new BeatmapInfoDetails(),
                            new BeatmapInfoFooter(),
                        },
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Horizontal = 50 },
                        Margin = new MarginPadding { Vertical = 10 },
                        Spacing = new Vector2(10f),
                        Children = new Drawable[]
                        {
                            new BeatmapInfoRankings(),
                            new BeatmapInfoComments(),
                        }
                    }
                }
            };
        }

        protected override Container CreateOverlayContainer() => new PopoverContainer();

        protected override BeatmapInfoHeader CreateHeader() => new BeatmapInfoHeader();

        private APIRequest? currentRequest;

        /// <summary>
        /// Fetches the beatmap set corresponding to the specified online ID and shows the overlay at the first difficulty.
        /// </summary>
        public void FetchAndShowBeatmapSet(int beatmapSetId)
        {
            updateBeatmap(null, null);

            var request = new GetBeatmapSetRequest(beatmapSetId);

            request.Success += set =>
            {
                updateBeatmap(set, set.Beatmaps.First());
                currentRequest = null;
            };

            API.Queue(currentRequest = request);

            Show();
        }

        /// <summary>
        /// Fetches the beatmap corresponding to the specified online ID and shows the overlay.
        /// </summary>
        public void FetchAndShowBeatmap(int beatmapId)
        {
            updateBeatmap(null, null);

            var request = new GetBeatmapSetRequest(beatmapId, BeatmapSetLookupType.BeatmapId);

            request.Success += set =>
            {
                updateBeatmap(set, set.Beatmaps.Single(b => b.OnlineID == beatmapId));
                currentRequest = null;
            };

            API.Queue(currentRequest = request);

            Show();
        }

        /// <summary>
        /// Shows the beatmap info overlay displaying the first difficulty of the specified <see cref="APIBeatmapSet"/>.
        /// </summary>
        public void ShowBeatmapSet(APIBeatmapSet? set)
        {
            updateBeatmap(set, set?.Beatmaps.First());
            Show();
        }

        /// <summary>
        /// Shows the beatmap info overlay displaying the specified <see cref="APIBeatmap"/>.
        /// </summary>
        public void ShowBeatmap(APIBeatmap? info)
        {
            updateBeatmap(info?.BeatmapSet, info);
            Show();
        }

        protected override void PopOutComplete()
        {
            currentRequest?.Cancel();
            currentRequest = null;

            updateBeatmap(null, null);
        }

        /// <summary>
        /// Updates the current overlay bindables with the new provided <see cref="APIBeatmap"/> and <see cref="APIBeatmapSet"/>.
        /// </summary>
        private void updateBeatmap(APIBeatmapSet? set, APIBeatmap? info)
        {
            ruleset.UnbindEvents();

            beatmapSet.Value = set;
            ruleset.Value = info?.Ruleset;

            updateAvailableBeatmaps();

            beatmap.Value = info;

            Loading.State.Value = info == null ? Visibility.Visible : Visibility.Hidden;

            ruleset.ValueChanged += _ =>
            {
                updateAvailableBeatmaps();

                if (!availableBeatmaps.Value.Contains(beatmap.Value))
                    beatmap.Value = availableBeatmaps.Value.FirstOrDefault(b => b.Ruleset.OnlineID == ruleset.Value?.OnlineID);
            };
        }

        private void updateAvailableBeatmaps()
        {
            if (beatmapSet.Value == null || ruleset.Value == null)
            {
                availableBeatmaps.Value = Array.Empty<APIBeatmap>();
                return;
            }

            availableBeatmaps.Value = beatmapSet.Value.Beatmaps
                                                .Where(b => b.Ruleset.OnlineID == ruleset.Value.OnlineID)
                                                .OrderBy(b => b.Ruleset.OnlineID)
                                                .ToList();
        }
    }
}
