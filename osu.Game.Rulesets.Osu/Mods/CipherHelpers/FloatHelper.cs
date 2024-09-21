// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public class FloatHelper
    {
        public static float ReplaceFraction(float input, string newFraction)
        {
            string[] parts = input.ToString().Split(".");
            return float.Parse($"{parts[0]}.{newFraction}");
        }
    }
}
