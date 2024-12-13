// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class BitEncoder
    {
        public static readonly string FIRST_FRAME_KEY = "1011111110010111100111110111100010111111111100101010101100111110";
        private bool wroteSecondFrame;
        private bool wroteFirstFrame;
        private readonly Random random = new Random();

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, int? mask)
        {
            if (mask == null) return mousePosition;

            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition, ref input, mask.Value);
                wroteSecondFrame = true;
                return mousePosition;
            }

            if (!pressedActions) transformNthFrame(ref mousePosition, pressedActions, ref input, mask.Value);
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

        private void transformSecondFrame(ref Vector2 mousePosition, ref InputHelper input, int mask)
        {
            int plainTextLength = input.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);
            string maskBinary = Convert.ToString(mask, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, maskBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition, bool pressedActions, ref InputHelper input, int mask)
        {
            if (mask == 0) return;
            bool bitsLeftToEncode = input.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                bool toEncode = !pressedActions && random.Next(2) != 0;

                if (toEncode)
                {
                    string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                    FloatHelper.SetNthMantissaBit(ref xMantissaBits, 0, '1');
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);
                    int mask1SAmount = IntHelper.GetAmountOf1SInMask(mask);
                    string messageBits = input.GetBits(mask1SAmount);
                    string yMantissaBits = FloatHelper.GetMantissaBits(mousePosition.Y);
                    FloatHelper.SetMantissaBitsWithMask(ref yMantissaBits, mask, messageBits);
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
    }

    public class BitDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int messageLength;
        private int mask;
        private int frameIndex;

        public void ProcessFrame(object frame)
        {
            var fieldInfo = frame.GetType().GetField("Position");
            Vector2 position = (Vector2)fieldInfo.GetValue(frame);

            if (frameIndex == 0)
            {
                frameIndex++;
                return;
            }

            if (frameIndex == 1)
            {
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBits(position.X));
                string maskString = FloatHelper.GetMantissaBits(position.Y);
                mask = IntHelper.ParseBitString(maskString);
                frameIndex++;
                return;
            }

            if (readBits.Length < messageLength)
            {
                string xMantissaBits = FloatHelper.GetMantissaBits(position.X);
                char xBit = FloatHelper.GetNthMantissaBit(ref xMantissaBits, 0);

                if (xBit == '1')
                {
                    string yMantissaBits = FloatHelper.GetMantissaBits(position.Y);
                    string message = FloatHelper.GetMantissaBitsWithMask(ref yMantissaBits, mask);
                    readBits += message;
                }
            }

            frameIndex++;
        }

        public string GetDecodedMessage()
        {
            return StringHelper.ParseBitString(readBits);
        }
    }
}
