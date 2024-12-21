// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Interfaces;
using osuTK;
using FloatHelper = Cipher.Helpers.FloatHelper;
using InputHelper = Cipher.Helpers.InputHelper;
using IntHelper = Cipher.Helpers.IntHelper;
using StringHelper = Cipher.Helpers.StringHelper;

namespace Cipher.Transformers
{
    public class HalvesEncoder
    {
        public static readonly string FIRST_FRAME_KEY = "1011111110010101100111110111100010111111111100101010101100101010";
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

            if (!pressedActions) transformNthFrame(ref mousePosition, ref input);
            return mousePosition;
        }

        private void transformFirstFrame(ref Vector2 mousePosition)
        {
            // Split FirstFrameKey into two parts
            string xBits = FIRST_FRAME_KEY.Substring(0, 32);
            string yBits = FIRST_FRAME_KEY.Substring(32, 32);

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

        public void ProcessFrame(object frame)
        {
            var fieldInfo = frame.GetType().GetField("Position");
            Vector2 position = (Vector2)fieldInfo.GetValue(frame);

            if (frameIndex == 0)
            {
                string xBits = FloatHelper.GetFloatBits(position.X);
                string yBits = FloatHelper.GetFloatBits(position.Y);
                string frameKey = xBits + yBits;

                if (frameKey == HalvesEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

                return;
            }

            if (frameIndex == 1)
            {
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBits(position.X));
                Console.WriteLine($"Message length={messageLength}");
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
            return (HalvesDecoder)MemberwiseClone();
        }
    }
}
