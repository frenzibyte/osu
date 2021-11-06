// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSetV2.Info.Metadata;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    /// <summary>
    /// Displays the basic information of an <see cref="BeatmapInfo"/> in a single line.
    /// </summary>
    public class BeatmapInfoTitleStrip : CompositeDrawable, IHasCurrentValue<APIBeatmap>
    {
        private readonly bool withRulesetAndStarDifficulty;
        private readonly bool withLengthAndBPM;
        private readonly BindableWithCurrent<APIBeatmap> current = new BindableWithCurrent<APIBeatmap>();

        public Bindable<APIBeatmap> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        private RulesetIcon rulesetIcon;
        private StarRatingDisplay starRatingDisplay;
        private OsuSpriteText versionText;

        private OsuTextFlowContainer lengthBpmPill;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        /// <summary>
        /// Creates a <see cref="BeatmapInfoTitleStrip"/>.
        /// </summary>
        /// <param name="withRulesetAndStarDifficulty">Whether the ruleset icon and star difficulty should appear at the beginning.</param>
        /// <param name="withLengthAndBPM">Whether a length and BPM pill should be displayed at the right-side of this drawable.</param>
        public BeatmapInfoTitleStrip(bool withRulesetAndStarDifficulty, bool withLengthAndBPM)
        {
            this.withRulesetAndStarDifficulty = withRulesetAndStarDifficulty;
            this.withLengthAndBPM = withLengthAndBPM;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7f),
                    Children = new Drawable[]
                    {
                        rulesetIcon = new RulesetIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = colourProvider.Content1,
                            Size = new Vector2(20f),
                            Alpha = withRulesetAndStarDifficulty ? 1 : 0,
                        },
                        starRatingDisplay = new StarRatingDisplay(default)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Alpha = withRulesetAndStarDifficulty ? 1 : 0,
                        },
                        versionText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Bottom = 3 },
                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                            Colour = colourProvider.Content1,
                        },
                    }
                },
                new BeatmapMetadataPill
                {
                    Alpha = withLengthAndBPM ? 1f : 0f,
                },
                new CircularContainer
                {
                    Masking = true,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.X,
                    Height = 20f,
                    Alpha = withLengthAndBPM ? 1f : 0f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        lengthBpmPill = new OsuTextFlowContainer(s => s.Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold))
                        {
                            TextAnchor = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Horizontal = 20f },
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(b =>
            {
                rulesetIcon.Current.Value = b.NewValue?.Ruleset;
                rulesetIcon.Alpha = b.NewValue != null ? 1 : 0;

                starRatingDisplay.Current.Value = new StarDifficulty(b.NewValue?.StarRating ?? 0, 0);
                starRatingDisplay.Alpha = b.NewValue != null ? 1 : 0;

                versionText.Text = b.NewValue?.DifficultyName;

                // todo: separate to own component, "BeatmapInfoMetadataPill" probably.
                lengthBpmPill.Clear();

                lengthBpmPill.AddText("Length", s => s.Colour = colourProvider.Content1);
                lengthBpmPill.AddArbitraryDrawable(Empty().With(s => s.Width = 7.5f));
                lengthBpmPill.AddText(b.NewValue == null ? "-" : TimeSpan.FromMilliseconds(b.NewValue.Length).ToString(@"m\:ss"), s => s.Colour = colourProvider.Content2);

                lengthBpmPill.AddArbitraryDrawable(Empty().With(s => s.Width = 20f));

                lengthBpmPill.AddText("BPM", s => s.Colour = colourProvider.Content1);
                lengthBpmPill.AddArbitraryDrawable(Empty().With(s => s.Width = 7.5f));
                lengthBpmPill.AddText(b.NewValue?.BPM.ToString(@"0.##") ?? "-", s => s.Colour = colourProvider.Content2);
            }, true);
        }
    }
}
