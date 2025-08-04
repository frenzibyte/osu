// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Foundation;
using osu.Framework.Logging;
using UIKit;

namespace osu.iOS
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UISceneDelegate
    {
        public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            Logger.Log("Cool stuff happening now");
        }

        public override void OpenUrlContexts(UIScene scene, NSSet<UIOpenUrlContext> urlContexts)
        {
            Logger.Log($"Open URL contexts called with:\n{urlContexts.Count}");
        }
    }
}
