// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

#nullable enable

namespace osu.Game.Overlays.BeatmapSetV2.Info.Metadata
{
    /// <summary>
    /// A <see cref="BeatmapMetadataItem"/> accepting a number to display along with a <see cref="Bar"/> on the right side of the item.
    /// </summary>
    /// <typeparam name="T">The number type.</typeparam>
    public class BeatmapNumberMetadataItem<T> : BeatmapMetadataItem
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible, IFormattable
    {
        private readonly string format;

        private readonly OsuSpriteText valueText;
        private readonly Bar valueBar;

        public T? Value
        {
            set
            {
                valueText.Text = value != null ? new LocalisableFormattableString(value, format) : (LocalisableString)"-";
                valueBar.Length = value?.ToSingle(NumberFormatInfo.InvariantInfo) ?? valueBar.MinLength;
            }
        }

        public T MinValue
        {
            set => valueBar.MinLength = value.ToSingle(NumberFormatInfo.InvariantInfo);
        }

        public T MaxValue
        {
            set => valueBar.MaxLength = value.ToSingle(NumberFormatInfo.InvariantInfo);
        }

        public BeatmapNumberMetadataItem(string name, string format = "")
            : base(name)
        {
            this.format = format;

            MainContainer.Clear(false);
            MainContainer.AddRange(new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Padding = new MarginPadding { Right = 10f },
                    Children = new[]
                    {
                        LabelText.With(l =>
                        {
                            l.Anchor = Anchor.CentreLeft;
                            l.Origin = Anchor.CentreLeft;
                            l.RelativeSizeAxes = Axes.X;
                            l.Width = 1f;
                        }),
                        valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold)
                        }
                    }
                },
                valueBar = new CircularBar
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.5f, 4f),
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            valueBar.BackgroundColour = colourProvider.Background6;
            valueBar.AccentColour = colourProvider.Highlight1;
        }
    }
}
