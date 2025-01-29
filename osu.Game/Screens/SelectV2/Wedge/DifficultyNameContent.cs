// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public partial class DifficultyNameContent : CompositeDrawable
    {
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText mappedByLabel = null!;
        private OsuHoverContainer mapperLink = null!;
        private OsuSpriteText mapperName = null!;

        private Data? value;

        public Data? Value
        {
            get => value;
            set
            {
                if (this.value == value)
                    return;

                this.value = value;
                updateDisplay();
            }
        }

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        public DifficultyNameContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    difficultyName = new TruncatingSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(size: 19.2f, weight: FontWeight.SemiBold),
                    },
                    mappedByLabel = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(size: 16.8f),
                    },
                    // This is not a `LinkFlowContainer` as there are single-frame layout issues when Update()
                    // is being used for layout, see https://github.com/ppy/osu-framework/issues/3369.
                    mapperLink = new MapperLinkContainer
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Child = mapperName = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 16.8f),
                        }
                    },
                }
            };
        }

        private void updateDisplay()
        {
            difficultyName.Text = value?.DifficultyName ?? string.Empty;
            mappedByLabel.Text = value != null ? " mapped by " : string.Empty;

            // TODO: should be the mapper of the guest difficulty, but that isn't stored correctly yet (see https://github.com/ppy/osu/issues/12965)
            mapperName.Text = value?.Mapper.Username ?? string.Empty;
            mapperLink.Action = () =>
            {
                if (value != null)
                    linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, value.Value.Mapper));
            };
        }

        protected override void Update()
        {
            base.Update();

            // truncate difficulty name when width exceeds bounds, prioritizing mapper name display
            difficultyName.MaxWidth = Math.Max(DrawWidth - mappedByLabel.DrawWidth
                                                         - mapperName.DrawWidth, 0);
        }

        public record struct Data(string DifficultyName, IUser Mapper);

        private partial class MapperLinkContainer : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }
    }
}
