// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;

namespace Cipher.Tests.Helpers
{
    [TestFixture]
    public class InputHelperTest
    {
        private InputHelper inputHelper { get; set; } = null!;

        [SetUp]
        public void SetUp()
        {
            inputHelper = new InputHelper("test");
        }

        [TearDown]
        public void TearDown()
        {
            inputHelper.ResetIndex();
        }

        [Test]
        public void TestConstructor()
        {
            Assert.AreEqual("01110100011001010111001101110100", inputHelper.GetBits());
        }

        [Test]
        public void TestGetBit()
        {
            Assert.AreEqual('0', inputHelper.GetBit());
            Assert.AreEqual('1', inputHelper.GetBit());
            Assert.AreEqual('1', inputHelper.GetBit());
            Assert.AreEqual('1', inputHelper.GetBit());
            Assert.AreEqual('0', inputHelper.GetBit());
            Assert.AreEqual('1', inputHelper.GetBit());
            Assert.AreEqual('0', inputHelper.GetBit());
            Assert.AreEqual('0', inputHelper.GetBit());
            Assert.AreEqual('0', inputHelper.GetBit());
        }

        [Test]
        public void TestGetBits_N()
        {
            Assert.AreEqual("01110100", inputHelper.GetBits(8));
            Assert.AreEqual("01100101", inputHelper.GetBits(8));
            Assert.AreEqual("01110011", inputHelper.GetBits(8));
            Assert.AreEqual("01110100", inputHelper.GetBits(8));
        }

        [Test]
        public void TestGetBitsShouldPadWithZeros()
        {
            string bits = inputHelper.GetBits(40);

            Assert.AreEqual(40, bits.Length);
            Assert.AreEqual("0111010001100101011100110111010000000000", bits);
            Assert.IsTrue(bits.EndsWith("0000", StringComparison.Ordinal));
        }

        [Test]
        public void TestAreBitsLeft()
        {
            Assert.IsTrue(inputHelper.AreBitsLeft(32));
            Assert.IsFalse(inputHelper.AreBitsLeft(33));
        }
    }
}
