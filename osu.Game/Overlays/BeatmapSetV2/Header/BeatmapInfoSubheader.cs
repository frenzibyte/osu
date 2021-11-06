// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Header
{
    public class BeatmapInfoSubheader : CompositeDrawable
    {
        private OsuSpriteText title;
        private OsuSpriteText artist;
        private LinkFlowContainer creatorLinkFlow;

        [Resolved]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Top = 10f, Horizontal = 50f },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5f),
                            Children = new[]
                            {
                                title = new OsuSpriteText
                                {
                                    AlwaysPresent = true,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = OsuFont.TorusAlternate.With(size: 24, weight: FontWeight.Regular),
                                },
                                artist = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = OsuFont.TorusAlternate.With(size: 18, weight: FontWeight.Regular),
                                }
                            }
                        },
                        creatorLinkFlow = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 12f, weight: FontWeight.Regular))
                        {
                            AutoSizeAxes = Axes.Both,
                            Colour = colourProvider.Content2,
                        },
                        new BeatmapPicker
                        {
                            Margin = new MarginPadding { Top = 10f },
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSet.BindValueChanged(b =>
            {
                if (b.NewValue != null)
                {
                    title.Text = new RomanisableString(b.NewValue.TitleUnicode, b.NewValue.Title);
                    artist.Text = new RomanisableString(b.NewValue.ArtistUnicode, b.NewValue.Artist);
                }
                else
                {
                    title.Text = " ";
                    artist.Text = " ";
                }

                creatorLinkFlow.Clear();

                if (b.NewValue != null)
                {
                    creatorLinkFlow.AddText("created by ");
                    creatorLinkFlow.AddUserLink(b.NewValue.Author);
                }
                else
                    creatorLinkFlow.AddText(" ");
            }, true);
        }
    }
}
