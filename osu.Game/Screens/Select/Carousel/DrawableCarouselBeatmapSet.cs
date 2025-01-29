﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class DrawableCarouselBeatmapSet : DrawableCarouselItem, IHasContextMenu
    {
        public const float HEIGHT = MAX_HEIGHT;
        private const float arrow_container_width = 20;
        private const float difficulty_icon_container_width = 30;
        private const float corner_radius = 10;

        private Action<BeatmapSetInfo> restoreHiddenRequested = null!;
        private Action<int>? viewDetails;
        private Action<BeatmapInfo>? selectRequested;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private IBindable<StarDifficulty?> starDifficultyBindable = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public IReadOnlyList<DrawableCarouselItem> DrawableBeatmaps => beatmapContainer?.IsLoaded != true ? Array.Empty<DrawableCarouselItem>() : beatmapContainer;

        private Container<DrawableCarouselItem>? beatmapContainer;

        private BeatmapSetInfo beatmapSet = null!;

        private Task? beatmapsLoadTask;
        private Box colourBox = null!;
        private Container backgroundContainer = null!;
        private SetPanelContent mainFlow = null!;
        private Box backgroundPlaceholder = null!;

        private MenuItem[]? mainMenuItems;

        private double timeSinceUnpool;

        [Resolved]
        private BeatmapManager manager { get; set; } = null!;

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            Item = null;
            timeSinceUnpool = 0;

            ClearTransforms();
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapSetOverlay? beatmapOverlay, SongSelect? songSelect)
        {
            if (songSelect != null)
                mainMenuItems = songSelect.CreateForwardNavigationMenuItemsForBeatmap(() => (((CarouselBeatmapSet)Item!).GetNextToSelect() as CarouselBeatmap)!.BeatmapInfo);

            restoreHiddenRequested = s =>
            {
                foreach (var b in s.Beatmaps)
                    manager.Restore(b);
            };

            if (beatmapOverlay != null)
                viewDetails = beatmapOverlay.FetchAndShowBeatmapSet;

            if (songSelect != null)
                selectRequested = b => songSelect.FinaliseSelection(b);
        }

        protected override void Update()
        {
            base.Update();

            Debug.Assert(Item != null);

            // position updates should not occur if the item is filtered away.
            // this avoids panels flying across the screen only to be eventually off-screen or faded out.
            if (!Item.Visible) return;

            float targetY = Item.CarouselYPosition;

            if (Precision.AlmostEquals(targetY, Y))
                Y = targetY;
            else
                // algorithm for this is taken from ScrollContainer.
                // while it doesn't necessarily need to match 1:1, as we are emulating scroll in some cases this feels most correct.
                Y = (float)Interpolation.Lerp(targetY, Y, Math.Exp(-0.01 * Time.Elapsed));

            loadContentIfRequired();
        }

        private CancellationTokenSource? loadCancellation;

        protected override void UpdateItem()
        {
            loadCancellation?.Cancel();
            loadCancellation = null;

            base.UpdateItem();

            Content.Clear();
            Header.Clear();

            beatmapContainer = null;
            beatmapsLoadTask = null;

            if (Item == null)
                return;

            beatmapSet = ((CarouselBeatmapSet)Item).BeatmapSet;
        }

        private const float duration = 500;

        protected override void Deselected()
        {
            base.Deselected();

            MovementContainer.MoveToX(0, duration, Easing.OutQuint);

            updateBeatmapYPositions();
            updateSelectionState();
        }

        protected override void Selected()
        {
            base.Selected();

            MovementContainer.MoveToX(-100, duration, Easing.OutQuint);

            updateBeatmapDifficulties();
            updateSelectionState();
        }

        private void updateSelectionState()
        {
            // Header children may not be loaded yet.
            if (Header.Count == 0) return;

            float colourBoxWidth = beatmapSet.Beatmaps.Count == 1 ? difficulty_icon_container_width : arrow_container_width;

            var state = Item?.State.Value;

            backgroundContainer.Height = state == CarouselItemState.Selected ? HEIGHT - 4 : HEIGHT;

            // TODO: implement colour sampling of beatmap background for colour box and offset this by 10, hide for now
            backgroundContainer.MoveToX(state == CarouselItemState.Selected ? colourBoxWidth : 0, duration, Easing.OutQuint);
            mainFlow.MoveToX(state == CarouselItemState.Selected ? colourBoxWidth : 0, duration, Easing.OutQuint);

            colourBox.RelativeSizeAxes = state == CarouselItemState.Selected ? Axes.Both : Axes.Y;
            colourBox.Width = state == CarouselItemState.Selected ? 1 : colourBoxWidth + corner_radius;
            colourBox.FadeTo(state == CarouselItemState.Selected ? 1 : 0, duration, Easing.OutQuint);
            iconContainer.FadeTo(state == CarouselItemState.Selected ? 1 : 0, duration, Easing.OutQuint);
            backgroundPlaceholder.FadeTo(state == CarouselItemState.Selected ? 1 : 0, duration, Easing.OutQuint);

            starDifficultyCancellationSource?.Cancel();

            if (beatmapSet.Beatmaps.Count == 1)
            {
                starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmapSet.Beatmaps.Single(), (starDifficultyCancellationSource = new CancellationTokenSource()).Token);
                starDifficultyBindable.BindValueChanged(d =>
                {
                    // We want to update the EdgeEffect here instead of in selected() to make sure the colours are correct
                    if (d.NewValue != null)
                    {
                        colourBox.Colour = colours.ForStarDifficulty(d.NewValue.Value.Stars);
                        iconContainer.Colour = d.NewValue.Value.Stars > 6.5f ? Colour4.White : colourProvider.Background5;

                        Header.EffectContainer.EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = state == CarouselItemState.Selected ? colours.ForStarDifficulty(d.NewValue.Value.Stars).Opacity(0.5f) : Colour4.Black.Opacity(100),
                            Radius = state == CarouselItemState.Selected ? 15 : 10,
                        };
                    }
                }, true);
            }
            else
            {
                Header.EffectContainer.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = state == CarouselItemState.Selected ? Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f) : Colour4.Black.Opacity(100),
                    Radius = state == CarouselItemState.Selected ? 15 : 10,
                };
            }
        }

        private void updateBeatmapDifficulties()
        {
            Debug.Assert(Item != null);

            var carouselBeatmapSet = (CarouselBeatmapSet)Item;

            var visibleBeatmaps = carouselBeatmapSet.Items.Where(c => c.Visible).ToArray();

            // if we are already displaying all the correct beatmaps, only run animation updates.
            // note that the displayed beatmaps may change due to the applied filter.
            // a future optimisation could add/remove only changed difficulties rather than reinitialise.
            if (beatmapContainer != null && visibleBeatmaps.Length == beatmapContainer.Count && visibleBeatmaps.All(b => beatmapContainer.Any(c => c.Item == b)))
            {
                updateBeatmapYPositions();
            }
            else
            {
                if (beatmapSet.Beatmaps.Count == 1)
                    return;

                // on selection we show our child beatmaps.
                // for now this is a simple drawable construction each selection.
                // can be improved in the future.
                beatmapContainer = new Container<DrawableCarouselItem>
                {
                    X = 100,
                    RelativeSizeAxes = Axes.Both,
                    ChildrenEnumerable = visibleBeatmaps.Select(c => c.CreateDrawableRepresentation()!)
                };

                beatmapsLoadTask = LoadComponentAsync(beatmapContainer, loaded =>
                {
                    // make sure the pooled target hasn't changed.
                    if (beatmapContainer != loaded)
                        return;

                    Content.Child = loaded;
                    updateBeatmapYPositions();
                });
            }
        }

        [Resolved]
        private BeatmapCarousel.CarouselScrollContainer scrollContainer { get; set; } = null!;

        private void loadContentIfRequired()
        {
            Quad containingSsdq = scrollContainer.ScreenSpaceDrawQuad;

            // Using DelayedLoadWrappers would only allow us to load content when on screen, but we want to preload while off-screen
            // to provide a better user experience.

            // This is tracking time that this drawable is updating since the last pool.
            // This is intended to provide a debounce so very fast scrolls (from one end to the other of the carousel)
            // don't cause huge overheads.
            //
            // We increase the delay based on distance from centre, so the beatmaps the user is currently looking at load first.
            float timeUpdatingBeforeLoad = 50 + Math.Abs(containingSsdq.Centre.Y - ScreenSpaceDrawQuad.Centre.Y) / containingSsdq.Height * 100;

            Debug.Assert(Item != null);

            // A load is already in progress if the cancellation token is non-null.
            if (loadCancellation != null)
                return;

            timeSinceUnpool += Time.Elapsed;

            // We only trigger a load after this set has been in an updating state for a set amount of time.
            if (timeSinceUnpool <= timeUpdatingBeforeLoad)
                return;

            loadCancellation = new CancellationTokenSource();

            LoadComponentsAsync(new Drawable[]
            {
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        colourBox = new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Alpha = 0,
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                        backgroundContainer = new Container
                        {
                            Masking = true,
                            CornerRadius = corner_radius,
                            RelativeSizeAxes = Axes.X,
                            MaskingSmoothness = 2,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                backgroundPlaceholder = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                    Alpha = 0,
                                },
                                // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
                                new SetPanelBackground(manager.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID)))
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Shear = -CarouselHeader.SHEAR,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                },
                            },
                        },
                    }
                },
                iconContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Shear = -CarouselHeader.SHEAR,
                    Alpha = 0,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.CentreLeft,
                },
                mainFlow = new SetPanelContent((CarouselBeatmapSet)Item)
                {
                    RelativeSizeAxes = Axes.Both
                },
            }, drawables =>
            {
                Header.AddRange(drawables);
                drawables.ForEach(d => d.FadeInFromZero(150));
                updateSelectionState();

                if (beatmapSet.Beatmaps.Count == 1)
                {
                    iconContainer.Add(new ConstrainedIconContainer
                    {
                        X = difficulty_icon_container_width / 2,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = beatmapSet.Beatmaps.Single().Ruleset.CreateInstance().CreateIcon(),
                        Size = new Vector2(20),
                    });
                }
                else
                {
                    iconContainer.Add(new SpriteIcon
                    {
                        X = arrow_container_width / 2,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.Solid.ChevronRight,
                        Size = new Vector2(12),
                        // TODO: implement colour sampling of beatmap background
                        Colour = colourProvider.Background5,
                    });
                }
            }, loadCancellation.Token);
        }

        private CancellationTokenSource? starDifficultyCancellationSource;
        private Container iconContainer = null!;

        private void updateBeatmapYPositions()
        {
            if (beatmapContainer == null)
                return;

            if (beatmapsLoadTask == null || !beatmapsLoadTask.IsCompleted)
                return;

            float yPos = DrawableCarouselBeatmap.CAROUSEL_BEATMAP_SPACING;

            bool isSelected = Item?.State.Value == CarouselItemState.Selected;

            foreach (var panel in beatmapContainer)
            {
                Debug.Assert(panel.Item != null);

                if (isSelected)
                {
                    panel.MoveToY(yPos, 800, Easing.OutQuint);
                    yPos += panel.Item.TotalHeight;
                }
                else
                    panel.MoveToY(0, 800, Easing.OutQuint);
            }
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                Debug.Assert(beatmapSet != null);

                List<MenuItem> items = new List<MenuItem>();

                if (Item?.State.Value == CarouselItemState.NotSelected)
                    items.Add(new OsuMenuItem("Expand", MenuItemType.Highlighted, () => Item.State.Value = CarouselItemState.Selected));

                if (mainMenuItems != null)
                    items.AddRange(mainMenuItems);

                if (beatmapSet.OnlineID > 0 && viewDetails != null)
                    items.Add(new OsuMenuItem("Details...", MenuItemType.Standard, () => viewDetails(beatmapSet.OnlineID)));

                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(createCollectionMenuItem)
                                           .ToList();

                if (manageCollectionsDialog != null)
                    collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                items.Add(new OsuMenuItem("Collections") { Items = collectionItems });

                if (beatmapSet.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem("Restore all hidden", MenuItemType.Standard, () => restoreHiddenRequested(beatmapSet)));

                if (beatmapSet.GetOnlineURL(api, ruleset.Value) is string url)
                    items.Add(new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => game?.CopyUrlToClipboard(url)));

                if (dialogOverlay != null)
                    items.Add(new OsuMenuItem("Delete...", MenuItemType.Destructive, () => dialogOverlay.Push(new BeatmapDeleteDialog(beatmapSet))));
                return items.ToArray();
            }
        }

        private MenuItem createCollectionMenuItem(BeatmapCollection collection)
        {
            Debug.Assert(beatmapSet != null);

            TernaryState state;

            int countExisting = beatmapSet.Beatmaps.Count(b => collection.BeatmapMD5Hashes.Contains(b.MD5Hash));

            if (countExisting == beatmapSet.Beatmaps.Count)
                state = TernaryState.True;
            else if (countExisting > 0)
                state = TernaryState.Indeterminate;
            else
                state = TernaryState.False;

            var liveCollection = collection.ToLive(realm);

            return new TernaryStateToggleMenuItem(collection.Name, MenuItemType.Standard, s =>
            {
                liveCollection.PerformWrite(c =>
                {
                    foreach (var b in beatmapSet.Beatmaps)
                    {
                        switch (s)
                        {
                            case TernaryState.True:
                                if (c.BeatmapMD5Hashes.Contains(b.MD5Hash))
                                    continue;

                                c.BeatmapMD5Hashes.Add(b.MD5Hash);
                                break;

                            case TernaryState.False:
                                c.BeatmapMD5Hashes.Remove(b.MD5Hash);
                                break;
                        }
                    }
                });
            })
            {
                State = { Value = state }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Item?.State.Value == CarouselItemState.Selected && beatmapSet.Beatmaps.Count == 1)
                selectRequested?.Invoke(beatmapSet.Beatmaps.Single());

            return base.OnClick(e);
        }
    }
}
