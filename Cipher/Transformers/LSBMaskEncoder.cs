// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class LSBMaskEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "000110101010110100011000111101";
        private bool wroteSecondFrame;
        private bool wroteFirstFrame;
        private readonly Random random = new Random();

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters)
        {
            int? mask = null;

            if (parameters != null && parameters.Length > 0 && parameters[0] is int?)
            {
                mask = (int?)parameters[0];
            }

            if (mask == null) return mousePosition;

            if (!wroteFirstFrame)
            {
                FrameHelper.TransformFirstFrame(ref mousePosition, FIRST_FRAME_KEY);
                wroteFirstFrame = true;
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition, ref input, mask.Value);
                wroteSecondFrame = true;
                return mousePosition;
            }

            transformNthFrame(ref mousePosition, pressedActions, ref input, mask.Value);
            return mousePosition;
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
                string mantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                FloatHelper.SetNthMantissaBit(ref mantissaBits, 0, '0');
                FloatHelper.ReplaceMantissaBits(ref mousePosition.X, mantissaBits);
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
            }
        }
    }

    public class LSBMaskDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int messageLength;
        private int mask;
        private int frameIndex;

        public void ProcessFrame(object frame)
        {
            Vector2 position = FrameHelper.GetPositionFromFrameObject(ref frame);

            if (frameIndex == 0)
            {
                string frameKey = FrameHelper.GetPotentialFirstFrameKey(ref position);

                if (frameKey == LSBMaskEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

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

        public IDecoder Clone()
        {
            return (LSBMaskDecoder)MemberwiseClone();
        }
    }
}
