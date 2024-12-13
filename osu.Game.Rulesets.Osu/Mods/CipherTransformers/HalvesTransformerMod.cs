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
        public override string FirstFrameKey => "1011111110010101100111110111100010111111111100101010101100101010";
        private bool wroteSecondFrame;

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            if (!WroteFirstFrame)
            {
                TransformFirstFrame(ref mousePosition);
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition);
                wroteSecondFrame = true;
                return mousePosition;
            }

            if (!pressedActions) transformNthFrame(ref mousePosition, pressedActions);
            return mousePosition;
        }

        private void transformSecondFrame(ref Vector2 mousePosition)
        {
            // Write data length to x
            int plainTextLength = Plaintext.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition, bool pressedActions)
        {
            bool bitsLeftToEncode = Plaintext.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                char xBit = Plaintext.GetBit();
                char yBit = Plaintext.GetBit();
                if (xBit == '1')
                    FloatHelper.ReplaceFraction(ref mousePosition.X, "5");
                else
                    FloatHelper.ReplaceFraction(ref mousePosition.X, "0");
                if (yBit == '1')
                    FloatHelper.ReplaceFraction(ref mousePosition.Y, "5");
                else
                    FloatHelper.ReplaceFraction(ref mousePosition.Y, "0");
            }
        }

        public override string Decode(List<ReplayFrame> frames)
        {
            List<OsuReplayFrame> replayFrames = frames.Cast<OsuReplayFrame>().ToList();
            string readBits = string.Empty;
            int messageLength = 0;
            int i = 0;

            foreach (var frame in replayFrames)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                if (i == 1)
                {
                    messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBits(frame.Position.X));
                    i++;
                    continue;
                }

                bool toRead = readBits.Length < messageLength;

                if (toRead)
                {
                    Vector2 position = frame.Position;
                    string xFraction = FloatHelper.GetFraction(ref position.X);
                    string yFraction = FloatHelper.GetFraction(ref position.Y);

                    switch (xFraction)
                    {
                        case "5":
                            readBits += "1";
                            break;

                        case "0":
                            readBits += "0";
                            break;
                    }

                    switch (yFraction)
                    {
                        case "5":
                            readBits += "1";
                            break;

                        case "0":
                            readBits += "0";
                            break;
                    }
                }

                if (readBits.Length == messageLength)
                {
                    break;
                }
            }

            string decodedMessage = StringHelper.ParseBitString(readBits);
            return decodedMessage;
        }
    }
}
