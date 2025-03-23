// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;
using FloatHelper = Cipher.Helpers.FloatHelper;
using InputHelper = Cipher.Helpers.InputHelper;
using IntHelper = Cipher.Helpers.IntHelper;
using StringHelper = Cipher.Helpers.StringHelper;

namespace Cipher.Transformers
{
    public class FractionsEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "010010011101101001010001111011";
        public static readonly string[] ZERO_FRACTIONS = { "0" };
        public static readonly string[] ONE_FRACTIONS = { "5" };
        private bool wroteFirstFrame;
        private bool wroteSecondFrame;

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters)
        {
            if (!FrameHelper.IsFrameGood(mousePosition, pressedActions))
            {
                return mousePosition;
            }

            if (!wroteFirstFrame)
            {
                FrameHelper.TransformFirstFrame(ref mousePosition, FIRST_FRAME_KEY);
                wroteFirstFrame = true;
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition, ref input);
                wroteSecondFrame = true;
                return mousePosition;
            }

            transformNthFrame(ref mousePosition, ref input);
            return mousePosition;
        }

        private void transformSecondFrame(ref Vector2 mousePosition, ref InputHelper input)
        {
            int plainTextLength = input.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(15, '0');
            string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
            FloatHelper.SetMantissaBitsWithMask(ref xMantissaBits, FrameHelper.FifteenBitsMantissaMask, plainTextLengthBinary);
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);
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

    public class FractionsDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int messageLength;
        private int frameIndex;

        public void ProcessFrame(object frame)
        {
            Vector2 position = FrameHelper.GetPositionFromFrameObject(ref frame);
            IList actions = FrameHelper.GetActionsFromFrameObject(ref frame);

            if (!FrameHelper.IsFrameGood(position, actions))
            {
                return;
            }

            if (frameIndex == 0)
            {
                string frameKey = FrameHelper.GetPotentialFirstFrameKey(ref position);

                if (frameKey == FractionsEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

                return;
            }

            if (frameIndex == 1)
            {
                string xMantissaBits = FloatHelper.GetMantissaBits(position.X);
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBitsWithMask(ref xMantissaBits, FrameHelper.FifteenBitsMantissaMask));
                frameIndex++;
                return;
            }

            if (readBits.Length < messageLength)
            {
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

        public IDecoder Clone()
        {
            return (FractionsDecoder)MemberwiseClone();
        }
    }
}
