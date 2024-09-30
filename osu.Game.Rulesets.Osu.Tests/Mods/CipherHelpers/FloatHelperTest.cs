// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;

namespace osu.Game.Rulesets.Osu.Tests.Mods.CipherHelpers
{
    [TestFixture]
    public class FloatHelperTest
    {
        [Test]
        public void TestGetFloatBits()
        {
            Assert.AreEqual("00000000000000000000000000000000", FloatHelper.GetFloatBits(0.0f));
            Assert.AreEqual("01000000101110000000000000000000", FloatHelper.GetFloatBits(5.75f));
            Assert.AreEqual("11000000011000000000000000000000", FloatHelper.GetFloatBits(-3.5f));

            Assert.AreEqual("10000000000000000000000000000000", FloatHelper.GetFloatBits(-0.0f));
            Assert.AreEqual("01111111100000000000000000000000", FloatHelper.GetFloatBits(float.PositiveInfinity));
            Assert.AreEqual("11111111100000000000000000000000", FloatHelper.GetFloatBits(float.NegativeInfinity));
        }

        [Test]
        public void TestGetMantissaBits_Zeros()
        {
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(0f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(1f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(2f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(4f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(-1f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(-2f));
            Assert.AreEqual("00000000000000000000000", FloatHelper.GetMantissaBits(-4f));
        }

        [Test]
        public void TestGetMantissaBits_Halves()
        {
            Assert.AreEqual("10000000000000000000000", FloatHelper.GetMantissaBits(1.5f));
            Assert.AreEqual("01000000000000000000000", FloatHelper.GetMantissaBits(2.5f));
            Assert.AreEqual("11000000000000000000000", FloatHelper.GetMantissaBits(3.5f));
            Assert.AreEqual("00100000000000000000000", FloatHelper.GetMantissaBits(4.5f));
            Assert.AreEqual("01100000000000000000000", FloatHelper.GetMantissaBits(5.5f));
        }

        [Test]
        public void TestGetMantissaBits_Ones()
        {
            Assert.AreEqual("11111111111111111111111", FloatHelper.GetMantissaBits(1.99999988f));
        }

        [Test]
        public void TestReplaceMantissaBits_Valid_Smallest()
        {
            float input = 5.75f;
            string newMantissaBits = "00000000000000000000001";
            float expectedOutput = 4.0000004f;

            FloatHelper.ReplaceMantissaBits(ref input, newMantissaBits);

            Assert.AreEqual(expectedOutput, input, 0.000001f);
        }

        [Test]
        public void TestReplaceMantissaBits_Valid_Max()
        {
            float input = 1.5f;
            string newMantissaBits = "11111111111111111111111";
            float expectedOutput = 1.9999999f;

            FloatHelper.ReplaceMantissaBits(ref input, newMantissaBits);

            Assert.AreEqual(expectedOutput, input);
        }

        [Test]
        public void TestReplaceMantissaBits_InvalidMantissa_ThrowsArgumentException()
        {
            float input = 3.5f;
            string invalidMantissa = "10101";

            Assert.Throws<ArgumentException>(() => FloatHelper.ReplaceMantissaBits(ref input, invalidMantissa));
        }

        [Test]
        public void TestReplaceMantissaBits_ValidMantissa_ZeroMantissa()
        {
            float input = 10.0f;
            string newMantissaBits = "00000000000000000000000";
            float expectedOutput = 8.0f;

            FloatHelper.ReplaceMantissaBits(ref input, newMantissaBits);

            Assert.AreEqual(expectedOutput, input);
        }

        [Test]
        public void TestGetLastMantissaBits()
        {
            float input = 8.000049f;

            Assert.AreEqual("1", FloatHelper.GetLastMantissaBits(input, 1));
            Assert.AreEqual("11", FloatHelper.GetLastMantissaBits(input, 2));
            Assert.AreEqual("011", FloatHelper.GetLastMantissaBits(input, 3));
            Assert.AreEqual("0011", FloatHelper.GetLastMantissaBits(input, 4));
            Assert.AreEqual("10011", FloatHelper.GetLastMantissaBits(input, 5));
            Assert.AreEqual("110011", FloatHelper.GetLastMantissaBits(input, 6));
            Assert.AreEqual("00000000000000000110011", FloatHelper.GetLastMantissaBits(input, 23));
        }

        [Test]
        public void TestSetNthMantissaBit()
        {
            string mantissaBits = "00000000000000000000000";

            FloatHelper.SetNthMantissaBit(ref mantissaBits, 0, '1');
            Assert.AreEqual("00000000000000000000001", mantissaBits);

            FloatHelper.SetNthMantissaBit(ref mantissaBits, 1, '1');
            Assert.AreEqual("00000000000000000000011", mantissaBits);

            FloatHelper.SetNthMantissaBit(ref mantissaBits, 0, '0');
            Assert.AreEqual("00000000000000000000010", mantissaBits);
        }

        [Test]
        public void TestSetMantissaBitsWithMask()
        {
            int mask = 0b110011;
            string mantissaBits = "00000000000000000000000";
            string bitsToSet = "1010";

            FloatHelper.SetMantissaBitsWithMask(ref mantissaBits, mask, bitsToSet);
            Assert.AreEqual("00000000000000000100010", mantissaBits);
        }
    }
}
