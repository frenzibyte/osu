﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public partial class LetterboxOverlay : CompositeDrawable
    {
        public required BreakTracker BreakTracker { get; init; }

        private readonly Container fadeContainer;

        private readonly IBindable<Period?> currentPeriod = new Bindable<Period?>();

        private static readonly Color4 transparent_black = new Color4(0, 0, 0, 0);

        public LetterboxOverlay()
        {
            const int letterbox_height = 150;

            RelativeSizeAxes = Axes.Both;

            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = letterbox_height,
                        Colour = ColourInfo.GradientVertical(Color4.Black, transparent_black),
                    },
                    new Box
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = letterbox_height,
                        Colour = ColourInfo.GradientVertical(transparent_black, Color4.Black),
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            currentPeriod.BindTo(BreakTracker.CurrentPeriod);
            currentPeriod.BindValueChanged(updateDisplay, true);
        }

        private void updateDisplay(ValueChangedEvent<Period?> period)
        {
            FinishTransforms(true);
            Scheduler.CancelDelayedTasks();

            if (period.NewValue == null)
                return;

            var b = period.NewValue.Value;

            using (BeginAbsoluteSequence(b.Start))
            {
                fadeContainer.FadeIn(BreakOverlay.BREAK_FADE_DURATION);
                using (BeginDelayedSequence(b.Duration - BreakOverlay.BREAK_FADE_DURATION))
                    fadeContainer.FadeOut(BreakOverlay.BREAK_FADE_DURATION);
            }
        }
    }
}
