// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public class IntHelper
    {
        public static int GetAmountOf1SInMask(int mask)
        {
            int count = 0;

            while (mask > 0)
            {
                count += mask & 1;
                mask >>= 1;
            }

            return count;
        }

        public static int ParseBitString(string bitString)
        {
            int result = 0;

            for (int i = 0; i < bitString.Length; i++)
            {
                result <<= 1;
                result |= bitString[i] == '1' ? 1 : 0;
            }

            return result;
        }
    }
}
