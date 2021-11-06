// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Comments;

namespace osu.Game.Overlays.BeatmapSetV2
{
    public class BeatmapInfoComments : BeatmapInfoSection
    {
        [Resolved]
        private IBindable<APIBeatmapSet> beatmapSet { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            CommentsContainer comments;

            AddInternal(comments = new CommentsContainer());

            beatmapSet.BindValueChanged(set =>
            {
                if (set.NewValue != null)
                {
                    Show();
                    comments.ShowComments(CommentableType.Beatmapset, set.NewValue.OnlineID);
                }
                else
                    Hide();
            }, true);
        }
    }
}
