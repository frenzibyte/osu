// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public struct BeatmapSetOnlineGenre
    {
        [JsonProperty(@"id")]
        public int Id { get; set; }

        [JsonProperty(@"name")]
        public string Name { get; set; }
    }
}
