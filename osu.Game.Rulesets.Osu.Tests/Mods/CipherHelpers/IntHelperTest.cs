// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;

namespace osu.Game.Rulesets.Osu.Tests.Mods.CipherHelpers
{
    [TestFixture]
    public class IntHelperTest
    {
        [Test]
        public void TestGetAmountOf1SInMask()
        {
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(0), 0);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(1), 1);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(2), 1);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(3), 2);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(4), 1);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(5), 2);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(6), 2);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(7), 3);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(8), 1);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(9), 2);
            Assert.AreEqual(IntHelper.GetAmountOf1SInMask(64), 1);
        }

        [Test]
        public void TestParseBitString()
        {
            Assert.AreEqual(IntHelper.ParseBitString("0000"), 0);
            Assert.AreEqual(IntHelper.ParseBitString("0001"), 1);
            Assert.AreEqual(IntHelper.ParseBitString("0010"), 2);
            Assert.AreEqual(IntHelper.ParseBitString("0011"), 3);
            Assert.AreEqual(IntHelper.ParseBitString("0100"), 4);
            Assert.AreEqual(IntHelper.ParseBitString("0101"), 5);
            Assert.AreEqual(IntHelper.ParseBitString("0110"), 6);
            Assert.AreEqual(IntHelper.ParseBitString("0111"), 7);
            Assert.AreEqual(IntHelper.ParseBitString("1000"), 8);
        }
    }
}
