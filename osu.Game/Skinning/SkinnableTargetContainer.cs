// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public class SkinnableTargetContainer : SkinReloadableDrawable, ISkinnableTarget
    {
        private SkinnableTargetComponentsContainer? mainContent;
        private SkinnableTargetComponentsContainer? rulesetContent;

        public SkinnableTarget Target { get; }
        public Ruleset? Ruleset { get; }

        public IBindableList<ISkinnableDrawable> Components => components;

        private readonly BindableList<ISkinnableDrawable> components = new BindableList<ISkinnableDrawable>();

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks; // ensure that components are loaded even if the target container is hidden (ie. due to user toggle).

        public bool ComponentsLoaded { get; private set; }

        private CancellationTokenSource? cancellationSource;

        public SkinnableTargetContainer(SkinnableTarget target, Ruleset? ruleset = null)
        {
            Target = target;
            Ruleset = ruleset;
        }

        /// <summary>
        /// Reload all components in this container from the current skin.
        /// </summary>
        public void Reload()
        {
            ClearInternal();
            components.Clear();
            ComponentsLoaded = false;

            mainContent = CurrentSkin.GetDrawableComponent(new SkinnableTargetComponent(Target)) as SkinnableTargetComponentsContainer;
            rulesetContent = CurrentSkin.GetDrawableComponent(new SkinnableTargetComponent(Target, Ruleset)) as SkinnableTargetComponentsContainer;

            if (rulesetContent != null)
            {
                foreach (var component in rulesetContent.OfType<ISkinnableDrawable>())
                    component.RulesetName = Ruleset.AsNonNull().ShortName;
            }

            cancellationSource?.Cancel();
            cancellationSource = null;

            if (mainContent != null || rulesetContent != null)
            {
                LoadComponentsAsync(new[] { mainContent, rulesetContent }.Where(d => d != null), loaded =>
                {
                    AddRangeInternal(loaded);
                    components.AddRange(loaded.SelectMany(w => w.AsNonNull().Children.OfType<ISkinnableDrawable>()));

                    ComponentsLoaded = true;
                }, (cancellationSource = new CancellationTokenSource()).Token);
            }
            else
                ComponentsLoaded = true;
        }

        /// <inheritdoc cref="ISkinnableTarget"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Add(ISkinnableDrawable component)
        {
            var content = getContent(component);
            if (content == null)
                throw new NotSupportedException("Attempting to add a new component to a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Add(drawable);
            components.Add(component);
        }

        /// <inheritdoc cref="ISkinnableTarget"/>
        /// <exception cref="NotSupportedException">Thrown when attempting to add an element to a target which is not supported by the current skin.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided instance is not a <see cref="Drawable"/>.</exception>
        public void Remove(ISkinnableDrawable component)
        {
            var content = getContent(component);
            if (content == null)
                throw new NotSupportedException("Attempting to remove a new component from a target container which is not supported by the current skin.");

            if (!(component is Drawable drawable))
                throw new ArgumentException($"Provided argument must be of type {nameof(Drawable)}.", nameof(component));

            content.Remove(drawable, true);
            components.Remove(component);
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            Reload();
        }

        private SkinnableTargetComponentsContainer? getContent(ISkinnableDrawable component) => component.RulesetName != null ? rulesetContent : mainContent;
    }
}
