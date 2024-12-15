// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Cipher.Helpers;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods.CipherTransformers;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods.CipherMods
{
    [TestFixture]
    public class BitEncoderTransformerModTest
    {
        private const string test_message = "Hello, Cipher!";
        private const int frame_count = 200;

        private static List<OsuReplayFrame> generateReplayFrames(int count)
        {
            var frames = new List<OsuReplayFrame>();

            for (int i = 0; i < count; i++)
            {
                frames.Add(new OsuReplayFrame(1, new Vector2(0, 0)));
            }

            return frames;
        }

        [Test]
        public void TestEndToEnd()
        {
            var transformer = new BitEncoderTransformerMod();
            transformer.Mask.Value = 0b11111111;
            transformer.Plaintext = new InputHelper(test_message);

            List<OsuReplayFrame> frames = generateReplayFrames(frame_count);

            for (int i = 0; i < frames.Count; i++)
            {
                Vector2 position = frames[i].Position;
                Vector2 transformedPosition = transformer.Transform(position, false);
                frames[i] = new OsuReplayFrame(frames[i].Time, transformedPosition);
            }

            string decodedMessage = transformer.Decode(new List<ReplayFrame>(frames));

            Assert.AreEqual(test_message, decodedMessage);
        }
    }
}
