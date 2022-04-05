// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject>, IKeyBindingHandler<TaikoAction>
    {
        protected readonly Container Content;
        private readonly Container behindDrumContent;

        private readonly Container nonProxiedContent;

        protected DrawableTaikoHitObject([CanBeNull] TaikoHitObject hitObject)
            : base(hitObject)
        {
            AddRangeInternal(new[]
            {
                behindDrumContent = new ProxiedContentContainer { RelativeSizeAxes = Axes.Both },
                nonProxiedContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Content = new Container { RelativeSizeAxes = Axes.Both }
                },
            });
        }

        /// <summary>
        /// <see cref="behindDrumContent"/> is proxied into a lower layer. We don't want to get masked away otherwise <see cref="behindDrumContent"/> would too.
        /// </summary>
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        /// <summary>
        /// Whether <see cref="Content"/> has been moved to a layer behind the input drum.
        /// </summary>
        protected bool IsProxiedBehindInputDrum { get; private set; }

        /// <summary>
        /// Moves <see cref="Content"/> to a layer proxied behind the input drum.
        /// Does nothing if content is already proxied.
        /// </summary>
        protected void ProxyContent()
        {
            if (IsProxiedBehindInputDrum) return;

            IsProxiedBehindInputDrum = true;

            nonProxiedContent.Remove(Content);
            behindDrumContent.Add(Content);
        }

        /// <summary>
        /// Moves <see cref="Content"/> to the normal hitobject layer.
        /// Does nothing is content is not currently proxied.
        /// </summary>
        protected void UnproxyContent()
        {
            if (!IsProxiedBehindInputDrum) return;

            IsProxiedBehindInputDrum = false;

            behindDrumContent.Remove(Content);
            nonProxiedContent.Add(Content);
        }

        /// <summary>
        /// Creates a proxy for content to be rendered behind the input drum.
        /// </summary>
        public Drawable CreateBehindDrumProxy() => behindDrumContent.CreateProxy();

        public abstract bool OnPressed(KeyBindingPressEvent<TaikoAction> e);

        public virtual void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
        {
        }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                behindDrumContent.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                behindDrumContent.LifetimeEnd = value;
            }
        }

        private class ProxiedContentContainer : Container
        {
            public override bool RemoveWhenNotAlive => false;
        }
    }

    public abstract class DrawableTaikoHitObject<TObject> : DrawableTaikoHitObject
        where TObject : TaikoHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        public new TObject HitObject => (TObject)base.HitObject;

        protected Vector2 BaseSize;
        protected SkinnableDrawable MainPiece;

        protected DrawableTaikoHitObject([CanBeNull] TObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void OnApply()
        {
            base.OnApply();
            RecreatePieces();
        }

        protected virtual void RecreatePieces()
        {
            Size = BaseSize = new Vector2(TaikoHitObject.DEFAULT_SIZE);

            if (MainPiece != null)
                Content.Remove(MainPiece);

            Content.Add(MainPiece = CreateMainPiece());
        }

        // Most osu!taiko hitsounds are managed by the drum (see DrumSampleMapping).
        public override IEnumerable<HitSampleInfo> GetSamples() => Enumerable.Empty<HitSampleInfo>();

        protected abstract SkinnableDrawable CreateMainPiece();
    }
}
