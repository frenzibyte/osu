// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;
using CommonStrings = osu.Game.Localisation.CommonStrings;

namespace osu.Game.Overlays.BeatmapSetV2.Info
{
    public class BeatmapDownloadButton : CompositeDrawable, IHasCurrentValue<APIBeatmapSet>
    {
        private readonly BindableWithCurrent<APIBeatmapSet> current = new BindableWithCurrent<APIBeatmapSet>();

        public Bindable<APIBeatmapSet> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly IBindable<DownloadState> state = new Bindable<DownloadState>();

        private RoundedButton button;
        private BeatmapDownloadTracker downloadTracker;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = button = new RoundedButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(120f, 30f),
                Action = () =>
                {
                    switch (state.Value)
                    {
                        case DownloadState.NotDownloaded:
                            beatmaps.Download(Current.Value);
                            break;

                        case DownloadState.Downloading:
                        case DownloadState.Importing:
                            break;

                        case DownloadState.LocallyAvailable:
                            game?.PresentBeatmap(Current.Value);
                            break;
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            state.BindValueChanged(s =>
            {
                switch (s.NewValue)
                {
                    case DownloadState.NotDownloaded:
                        button.BackgroundColour = colours.Lime3;
                        button.Text = BeatmapsetsStrings.ShowDetailsDownloadDefault;
                        break;

                    case DownloadState.Downloading:
                        button.BackgroundColour = colours.Lime3;
                        button.Text = CommonStrings.Downloading;
                        break;

                    case DownloadState.Importing:
                        button.BackgroundColour = colours.Orange3;
                        button.Text = CommonStrings.Importing;
                        break;

                    case DownloadState.LocallyAvailable:
                        button.BackgroundColour = colours.Blue3;
                        button.Text = "Go to beatmap"; // todo: hmm
                        break;
                }
            }, true);

            Current.BindValueChanged(set =>
            {
                downloadTracker?.RemoveAndDisposeImmediately();
                downloadTracker = null;

                if (set.NewValue != null)
                {
                    AddInternal(downloadTracker = new BeatmapDownloadTracker(set.NewValue)
                    {
                        State = { BindTarget = state }
                    });

                    button.Enabled.Value = true;
                }
                else
                    button.Enabled.Value = false;
            }, true);
        }
    }
}
