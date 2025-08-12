// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning.Triangles;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class TrianglesOsuJudgementPiece : TrianglesJudgementPiece
    {
        public TrianglesOsuJudgementPiece(HitResult result)
            : base(result)
        {
        }

        public override void PlayAnimation()
        {
            if (Result != HitResult.Miss)
            {
                JudgementText
                    .ScaleTo(new Vector2(0.8f, 1))
                    .ScaleTo(new Vector2(1.2f, 1), 1800, Easing.OutQuint);
            }

            base.PlayAnimation();
        }
    }
}
