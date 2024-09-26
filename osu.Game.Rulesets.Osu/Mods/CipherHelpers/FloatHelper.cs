// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public class FloatHelper
    {
        /// <summary>
        /// Gets the mantissa bits of <paramref name="input"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMantissaBits(float input)
        {
            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);

            const int mantissa_mask = 0x007FFFFF;
            int mantissaBits = floatBits & mantissa_mask;

            string mantissaBinary = Convert.ToString(mantissaBits, 2).PadLeft(23, '0');

            return mantissaBinary;
        }

        /// <summary>
        /// Replaces mantissa of <paramref name="input"/> with <paramref name="newMantissaBits"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="newMantissaBits">New mantissa in bit format</param>
        public static void ReplaceMantissaBits(ref float input, string newMantissaBits)
        {
            if (newMantissaBits.Length != 23 || !System.Text.RegularExpressions.Regex.IsMatch(newMantissaBits, "^[01]+$"))
            {
                throw new ArgumentException("newMantissaBits must be 23 characters long and contain only 0s and 1s.");
            }

            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);
            const int mantissa_mask = 0x007FFFFF;
            int newMantissa = Convert.ToInt32(newMantissaBits, 2);
            floatBits &= ~mantissa_mask;
            floatBits |= newMantissa;
            input = BitConverter.ToSingle(BitConverter.GetBytes(floatBits), 0);
        }

        public static string GetLastMantissaBits(float input, int n)
        {
            if (n is < 1 or > 23)
            {
                throw new ArgumentException("n must be between 1 and 23.");
            }

            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);
            const int mantissa_mask = 0x007FFFFF;
            int mantissaBits = floatBits & mantissa_mask;
            int lastBits = mantissaBits & ((1 << n) - 1);
            string lastMantissaBits = Convert.ToString(lastBits, 2).PadLeft(n, '0');

            return lastMantissaBits;
        }

        public static string GetFloatBits(float input)
        {
            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);
            string binaryRepresentation = Convert.ToString(floatBits, 2).PadLeft(32, '0');
            return binaryRepresentation;
        }

        public static void ReplaceFraction(ref float input, string newFraction)
        {
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            string[] parts = input.ToString().Split(".");
            input = float.Parse($"{parts[0]}.{newFraction}");
        }
    }
}
