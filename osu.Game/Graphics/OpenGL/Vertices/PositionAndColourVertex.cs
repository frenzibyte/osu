// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Runtime.InteropServices;
using osu.Framework.Graphics.Rendering.Vertices;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;

namespace osu.Game.Graphics.OpenGL.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionAndColourVertex : IEquatable<PositionAndColourVertex>, IVertex
    {
        [VertexMember(2, VertexAttribPointerType.HalfFloat)]
        public Vector2h Position;

        [VertexMember(4, VertexAttribPointerType.HalfFloat)]
        public Vector4h Colour;

        public bool Equals(PositionAndColourVertex other)
            => Position.Equals(other.Position)
               && Colour.Equals(other.Colour);
    }
}
