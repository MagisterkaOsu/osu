// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class LetterMappingEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "1011111110010101100111110111100010111111111100101010101100110000";
        private bool wroteFirstFrame;
        private bool wroteSecondFrame;
        private readonly Random random = new Random();

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters)
        {
            int? position = null;

            if (parameters != null && parameters.Length > 0 && parameters[0] is int?)
            {
                position = (int?)parameters[0];
            }

            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
                return mousePosition;
            }

            if (!wroteSecondFrame)
            {
                transformSecondFrame(ref mousePosition, ref input, position.Value);
                wroteSecondFrame = true;
                return mousePosition;
            }

            transformNthFrame(ref mousePosition, pressedActions, ref input, position.Value);
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

        private void transformSecondFrame(ref Vector2 mousePosition, ref InputHelper input, int position)
        {
            int plainTextLength = input.GetLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);

            string positionBinary = Convert.ToString(position, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, positionBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition, bool pressedActions, ref InputHelper input, int position)
        {
            bool bitsLeftToEncode = input.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                bool toEncode = !pressedActions && random.Next(2) != 0;
                int ascii;

                if (toEncode)
                {
                    string letter = input.GetLetter();
                    ascii = letter[0] - 32;
                }
                else
                {
                    ascii = random.Next(95, 99);
                }

                string asciiIndex = ascii.ToString().PadLeft(2, '0');
                string xFractionalPart = FloatHelper.GetFraction(ref mousePosition.X).PadRight(position + 1, '0');
                string yFractionalPart = FloatHelper.GetFraction(ref mousePosition.Y).PadRight(position + 1, '0');
                xFractionalPart = $"{xFractionalPart.Substring(0, position)}{asciiIndex[0]}{xFractionalPart.Substring(position+1)}";
                yFractionalPart = $"{yFractionalPart.Substring(0, position)}{asciiIndex[1]}{yFractionalPart.Substring(position+1)}";
                FloatHelper.ReplaceFraction(ref mousePosition.X, xFractionalPart);
                FloatHelper.ReplaceFraction(ref mousePosition.Y, yFractionalPart);
            }
        }
    }

    public class LetterMappingDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int messageLength;
        private int position;
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

                if (frameKey == LetterMappingEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

                return;
            }

            if (frameIndex == 1)
            {
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBits(position.X));
                string positionString = FloatHelper.GetMantissaBits(position.Y);
                this.position = IntHelper.ParseBitString(positionString);
                frameIndex++;
                return;
            }

            if (readBits.Length < messageLength)
            {
                string xFraction = FloatHelper.GetFraction(ref position.X);
                string yFraction = FloatHelper.GetFraction(ref position.Y);
                string leftDigit = xFraction[this.position].ToString();
                string rightDigit = yFraction[this.position].ToString();
                int ascii = int.Parse($"{leftDigit}{rightDigit}");

                if (ascii < 95)
                {
                    char letter = (char)(ascii + 32);
                    string bits = Convert.ToString(letter, 2).PadLeft(8, '0');
                    readBits += bits;
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
            return (LetterMappingDecoder)MemberwiseClone();
        }
    }
}
