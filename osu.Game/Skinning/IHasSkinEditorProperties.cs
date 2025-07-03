// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Interface for disabling skin editor features on a specific component.
    /// </summary>
    public interface IHasSkinEditorProperties
    {
        /// <summary>
        /// Whether "closest" anchor can be selected.
        /// </summary>
        bool AllowsClosestAnchor { get; }

        /// <summary>
        /// The list of anchors allowed to be set to this component, or null if all anchors are allowed.
        /// </summary>
        Anchor[]? AllowedAnchors { get; }
    }
}
