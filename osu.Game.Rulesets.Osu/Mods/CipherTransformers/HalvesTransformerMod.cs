// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class HalvesTransformerMod : OsuModCipher
    {
        public override string Name => "Halves";
        public override string Acronym => "HV";
        public override LocalisableString Description => ".5 - '1' ; .0 - '0'";

        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;

        public override Vector2 Transform(Vector2 mousePosition)
        {
            if (!Plaintext.AreBitsLeft()) return mousePosition;

            char xBit = Plaintext.GetBit();
            char yBit = Plaintext.GetBit();

            Vector2 result = mousePosition;
            result.X = xBit == '1' ? FloatHelper.ReplaceFraction(mousePosition.X, "5") : FloatHelper.ReplaceFraction(mousePosition.X, "0");
            result.Y = yBit == '1' ? FloatHelper.ReplaceFraction(mousePosition.Y, "5") : FloatHelper.ReplaceFraction(mousePosition.Y, "0");
            return result;
        }
    }
}
