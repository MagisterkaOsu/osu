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
                throw new ArgumentException($"newMantissaBits must be 23 characters long and contain only 0s and 1s. Instead newMantissaBits={newMantissaBits}");
            }

            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);
            const int mantissa_mask = 0x007FFFFF;
            int newMantissa = Convert.ToInt32(newMantissaBits, 2);
            floatBits &= ~mantissa_mask;
            floatBits |= newMantissa;
            input = BitConverter.ToSingle(BitConverter.GetBytes(floatBits), 0);
        }

        public static void ReplaceBits(ref float input, string newBits)
        {
            if (newBits.Length != 32 || !System.Text.RegularExpressions.Regex.IsMatch(newBits, "^[01]+$"))
            {
                throw new ArgumentException($"newBits must be 32 characters long and contain only 0s and 1s. Instead newBits={newBits}");
            }

            int floatBits = BitConverter.ToInt32(BitConverter.GetBytes(input), 0);
            int newFloatBits = Convert.ToInt32(newBits, 2);
            input = BitConverter.ToSingle(BitConverter.GetBytes(newFloatBits), 0);
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

        public static char GetNthMantissaBit(ref string mantissaBits, int position)
        {
            if (position is < 0 or > 22)
            {
                throw new ArgumentException("position must be between 0 and 22.");
            }

            char[] bits = mantissaBits.ToCharArray();
            return bits[22 - position];
        }

        public static void SetNthMantissaBit(ref string mantissaBits, int position, char bit)
        {
            if (position is < 0 or > 22)
            {
                throw new ArgumentException("position must be between 0 and 22.");
            }

            if (bit != '0' && bit != '1')
            {
                throw new ArgumentException("bit must be either '0' or '1'.");
            }

            char[] bits = mantissaBits.ToCharArray();
            bits[22 - position] = bit;
            mantissaBits = new string(bits);
        }

        public static void SetMantissaBitsWithMask(ref string mantissaBits, int mask, string bitsMessage)
        {
            int maskBits = Convert.ToString(mask, 2).Replace("0", "").Length;

            if (maskBits != bitsMessage.Length)
            {
                throw new ArgumentException("Amount of 1s in mask must be equal to the length of bitsMessage.");
            }

            char[] bits = mantissaBits.ToCharArray();
            char[] bitsToSet = bitsMessage.ToCharArray();
            int maskIndex = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    bits[22 - i] = bitsToSet[bitsMessage.Length - maskIndex - 1];
                    maskIndex++;
                }
            }

            mantissaBits = new string(bits);
        }

        public static string GetMantissaBitsWithMask(ref string mantissaBits, int mask)
        {
            char[] bits = mantissaBits.ToCharArray();
            string output = String.Empty;
            int maskIndex = 0;

            for (int i = 0; i < bits.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    output = bits[22 - i] + output;
                    maskIndex++;
                }
            }

            return output;
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
