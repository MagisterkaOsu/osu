// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class BitEncoderTransformerMod : OsuModCipher
    {
        public override string Name => "Bit Encoder";
        public override string Acronym => "BE";
        public override LocalisableString Description => "Encodes data into mantissa bits";
        public override IconUsage? Icon => FontAwesome.Solid.QuestionCircle;

        [SettingSource("Mask", "Message mask", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Mask { get; } = new Bindable<int?>(0);

        public const string FirstFrameKey = "1011111110010111100111110111100010111111111100101010101100111110";
        private bool wroteFirstFrame;
        private bool wroteSecondFrame;
        private Random random = new Random();

        #region Encode

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            if (Mask.Value == null) return mousePosition;

            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
            }
            else if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition);
                wroteSecondFrame = true;
            }
            else if (!pressedActions) transformNthFrame(ref mousePosition, pressedActions);

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

        private void transformSecondFrame(ref Vector2 mousePosition)
        {
            // Write data length to x
            int plainTextLength = Plaintext.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);

            // Write mask to y
            string maskBinary = Convert.ToString((int)Mask.Value!, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, maskBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition, bool pressedActions)
        {
            if (Mask.Value == null || Mask.Value == 0) return;
            bool bitsLeftToEncode = Plaintext.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                bool toEncode = !pressedActions && random.Next(2) != 0;

                if (toEncode)
                {
                    string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                    FloatHelper.SetNthMantissaBit(ref xMantissaBits, 0, '1');
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);
                    int mask1SAmount = IntHelper.GetAmountOf1SInMask((int)Mask.Value);
                    string messageBits = Plaintext.GetBits(mask1SAmount);
                    string yMantissaBits = FloatHelper.GetMantissaBits(mousePosition.Y);
                    FloatHelper.SetMantissaBitsWithMask(ref yMantissaBits, (int)Mask.Value, messageBits);
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, yMantissaBits);
                }
                else
                {
                    string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                    FloatHelper.SetNthMantissaBit(ref xMantissaBits, 0, '0');
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);
                }
            }
        }
        #endregion

        #region Decode

        public override string Decode(List<ReplayFrame> frames)
        {
            List<OsuReplayFrame> replayFrames = frames.Cast<OsuReplayFrame>().ToList();
            string readBits = string.Empty;
            string decodedMessage = string.Empty;
            int i = 0;
            int messageLength = 0;
            int mask = 0;

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
                    string maskString = FloatHelper.GetMantissaBits(frame.Position.Y);
                    mask = IntHelper.ParseBitString(maskString);
                    i++;
                    continue;
                }

                string xMantissaBits = FloatHelper.GetMantissaBits(frame.Position.X);
                char xBit = FloatHelper.GetNthMantissaBit(ref xMantissaBits, 0);
                bool toRead = readBits.Length < messageLength && xBit == '1';

                if (toRead)
                {
                    string yMantissaBits = FloatHelper.GetMantissaBits(frame.Position.Y);
                    string message = FloatHelper.GetMantissaBitsWithMask(ref yMantissaBits, mask);
                    readBits += message;
                }
                else
                {
                    if (readBits.Length == messageLength)
                    {
                        break;
                    }
                }
            }

            decodedMessage = StringHelper.ParseBitString(readBits);
            return decodedMessage;
        }

        #endregion
    }
}
