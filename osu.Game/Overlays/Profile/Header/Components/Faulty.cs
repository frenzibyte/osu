// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Overlays.Profile
{
    public class Faulty
    {
        public void Method()
        {
            Action<Model> action = s =>
            {
                s.Death();
                Console.WriteLine("value: " + 3);
            };

            action(new Model());
        }
    }

    public class Model
    {
        public void Death(bool? parameter = null)
        {
        }
    }
}
