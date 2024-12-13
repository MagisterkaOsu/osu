// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using System.Linq;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class HalvesTransformerMod : OsuModCipher
    {
        public override string Name => "Halves";
        public override string Acronym => "HV";
        public override LocalisableString Description => ".5 - '1' ; .0 - '0'";
        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;
        public override string FirstFrameKey => HalvesEncoder.FirstFrameKey;

        private readonly HalvesEncoder encoder = new HalvesEncoder();
        private readonly HalvesDecoder decoder = new HalvesDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            return encoder.Encode(mousePosition, pressedActions, ref Plaintext);
        }

        public override string Decode(List<ReplayFrame> frames)
        {
            foreach (var frame in frames.Cast<OsuReplayFrame>())
            {
                decoder.ProcessFrame(frame);
            }

            return decoder.GetDecodedMessage();
        }
    }

    public class HalvesEncoder : IEncoder
    {
        public static string FirstFrameKey = "1011111110010101100111110111100010111111111100101010101100101010";

        private bool wroteFirstFrame;
        private bool wroteSecondFrame;

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input)
        {
            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition, ref input);
                wroteSecondFrame = true;
                return mousePosition;
            }

            if (!pressedActions)
                transformNthFrame(ref mousePosition, ref input);

            return mousePosition;
        }

        private void transformFirstFrame(ref Vector2 mousePosition)
        {
            // Split FirstFrameKey into two parts
            string xBits = FirstFrameKey.Substring(0, 32);
            string yBits = FirstFrameKey.Substring(32, 32);

            // Write the parts to the mantissas of X and Y
            FloatHelper.ReplaceBits(ref mousePosition.X, xBits);
            FloatHelper.ReplaceBits(ref mousePosition.Y, yBits);
        }

        private void transformSecondFrame(ref Vector2 mousePosition, ref InputHelper input)
        {
            int plainTextLength = input.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition, ref InputHelper input)
        {
            bool bitsLeftToEncode = input.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                char xBit = input.GetBit();
                char yBit = input.GetBit();

                FloatHelper.ReplaceFraction(ref mousePosition.X, xBit == '1' ? "5" : "0");
                FloatHelper.ReplaceFraction(ref mousePosition.Y, yBit == '1' ? "5" : "0");
            }
        }
    }

    public class HalvesDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int messageLength;
        private int frameIndex;
        private bool lengthDecoded;

        public void ProcessFrame(OsuReplayFrame frame)
        {
            if (frameIndex == 0)
            {
                frameIndex++;
                return;
            }

            if (!lengthDecoded)
            {
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBits(frame.Position.X));
                lengthDecoded = true;
                return;
            }

            if (readBits.Length < messageLength)
            {
                Vector2 position = frame.Position;
                string xFraction = FloatHelper.GetFraction(ref position.X);
                string yFraction = FloatHelper.GetFraction(ref position.Y);

                readBits += xFraction == "5" ? "1" : "0";
                readBits += yFraction == "5" ? "1" : "0";
            }

            frameIndex++;
        }

        public string GetDecodedMessage()
        {
            return StringHelper.ParseBitString(readBits);
        }
    }
}
