// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounterDisplay : KeyCounterDisplay
    {
        private const int duration = 100;

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        private KeyCounterTotalTrigger totalTrigger;

        public ArgonKeyCounterDisplay()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(2f, 0f),
                Children = new Drawable[]
                {
                    new Circle
                    {
                        Size = new Vector2(48f, 2f),
                        Margin = new MarginPadding { Top = ArgonKeyCounter.LINE_HEIGHT * ArgonKeyCounter.SCALE_FACTOR / 2f - 1f },
                        Colour = Color4.White.Opacity(0.5f),
                    },
                    KeyFlow = new FillFlowContainer<KeyCounter>
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Alpha = 0,
                        Spacing = new Vector2(2),
                    },
                    new SmoothPath
                    {
                        PathRadius = 1f,
                        Colour = Color4.White.Opacity(0.5f),
                        Margin = new MarginPadding { Top = ArgonKeyCounter.LINE_HEIGHT * ArgonKeyCounter.SCALE_FACTOR / 2f - 1f },
                        Vertices = PathApproximator.ApproximateBezier(new[]
                        {
                            // todo: this is silly but it works
                            new Vector2(0, 0),
                            new Vector2(13, 0),
                            new Vector2(13, 0),
                            new Vector2(13, 0),
                            new Vector2(52, 40),
                            new Vector2(52, 40),
                            new Vector2(52, 40),
                            new Vector2(86, 40),
                        }.Select(v => v * ArgonKeyCounter.SCALE_FACTOR).ToArray()),
                    },
                    new ArgonKeyCounter(totalTrigger = new KeyCounterTotalTrigger())
                    {
                        Margin = new MarginPadding { Left = -40 },
                    },
                    new SmoothPath
                    {
                        PathRadius = 1f,
                        Colour = Color4.White.Opacity(0.5f),
                        Margin = new MarginPadding { Top = ArgonKeyCounter.LINE_HEIGHT * ArgonKeyCounter.SCALE_FACTOR / 2f - 1f },
                        Vertices = PathApproximator.ApproximateBezier(new[]
                        {
                            // todo: this is silly but it works
                            new Vector2(0, 0),
                            new Vector2(13, 0),
                            new Vector2(13, 0),
                            new Vector2(13, 0),
                            new Vector2(52, 40),
                            new Vector2(52, 40),
                            new Vector2(52, 40),
                            new Vector2(86, 40),
                        }.Select(v => v * ArgonKeyCounter.SCALE_FACTOR).ToArray()),
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var trigger in KeyFlow.Children.Select(c => c.Trigger))
            {
                trigger.OnActivate += totalTrigger.Activate;
                trigger.OnDeactivate += totalTrigger.Deactivate;
            }
        }

        protected override void Update()
        {
            base.Update();

            Size = KeyFlow.Size;
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger) => new ArgonKeyCounter(trigger);

        protected override void UpdateVisibility()
            => KeyFlow.FadeTo(AlwaysVisible.Value || ConfigVisibility.Value ? 1 : 0, duration);

        private partial class KeyCounterTotalTrigger : InputTrigger
        {
            public KeyCounterTotalTrigger()
                : base("Total")
            {
            }

            public new void Activate(bool forwardPlayback) => base.Activate(forwardPlayback);
            public new void Deactivate(bool forwardPlayback) => base.Deactivate(forwardPlayback);
        }
    }
}
