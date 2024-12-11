// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;

namespace osu.Game.Rulesets.Osu.Tests.Mods.CipherHelpers
{
    [TestFixture]
    public class StringHelperTest
    {
        [Test]
        public void TestParseBitString()
        {
            string bitString = "01010100011001010111001101110100";
            Assert.AreEqual(StringHelper.ParseBitString(bitString), "Test");
        }
    }
}
