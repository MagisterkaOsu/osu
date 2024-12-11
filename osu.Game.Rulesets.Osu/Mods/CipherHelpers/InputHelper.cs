// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public class InputHelper
    {
        private readonly string bitRepresentation;
        private int currentIndex;

        public InputHelper(string inputString)
        {
            bitRepresentation = string.Empty;
            currentIndex = 0;

            foreach (char c in inputString)
            {
                bitRepresentation += Convert.ToString(c, 2).PadLeft(8, '0');
            }
        }

        public char GetBit()
        {
            if (currentIndex >= bitRepresentation.Length)
            {
                Logger.Log("Whole plaintext was read. Supplying '0's from now on.");
                return '0';
            }

            return bitRepresentation[currentIndex++];
        }

        public string GetBits(int length)
        {
            int bitsToRead = length;

            if (!AreBitsLeft(length))
            {
                Logger.Log("Whole plaintext was read. Supplying '0's from now on.");
                bitsToRead = bitRepresentation.Length - currentIndex;
            }

            string bits = bitRepresentation.Substring(currentIndex, bitsToRead);
            currentIndex += bitsToRead;

            return bits.PadRight(length, '0');
        }

        public string GetBits()
        {
            return GetBits(bitRepresentation.Length - currentIndex);
        }

        public void ResetIndex()
        {
            currentIndex = 0;
        }

        public int GetLength()
        {
            return bitRepresentation.Length;
        }

        public bool AreBitsLeft(int amount = 1)
        {
            return currentIndex + amount <= bitRepresentation.Length;
        }
    }
}
