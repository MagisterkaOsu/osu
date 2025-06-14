// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class LetterMappingEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "101010000101011010000110000100";
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
                transformSecondFrame(ref mousePosition, ref input, position.Value);
                wroteSecondFrame = true;
                return mousePosition;
            }

            transformNthFrame(ref mousePosition, pressedActions, ref input, position.Value);
            return mousePosition;
        }

        private void transformSecondFrame(ref Vector2 mousePosition, ref InputHelper input, int position)
        {
            int plainTextLength = input.GetLetterLength();
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(15, '0');
            string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
            FloatHelper.SetMantissaBitsWithMask(ref xMantissaBits, FrameHelper.FifteenBitsMantissaMask, plainTextLengthBinary);
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);

            string positionBinary = Convert.ToString(position, 2).PadLeft(3, '0');
            string yMantissaBits = FloatHelper.GetMantissaBits(mousePosition.Y);
            FloatHelper.SetMantissaBitsWithMask(ref yMantissaBits, FrameHelper.ThreeBitsMantissaMask, positionBinary);
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, yMantissaBits);
        }

        private void transformNthFrame(ref Vector2 mousePosition, bool pressedActions, ref InputHelper input, int position)
        {
            bool bitsLeftToEncode = input.AreBitsLeft();

            if (bitsLeftToEncode)
            {
                bool toEncode = random.Next(2) != 0;
                int ascii;

                if (toEncode)
                {
                    string letter = input.GetLetter();
                    ascii = letter[0] - 32;
                    Console.WriteLine($"Letter: {letter} ({ascii})");
                }
                else
                {
                    ascii = random.Next(95, 100);
                    Console.WriteLine($"Letter: - ({ascii})");
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
        private string readString = string.Empty;
        private int messageLength;
        private int position;
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

                if (frameKey == LetterMappingEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

                return;
            }

            if (frameIndex == 1)
            {
                string xMantissaBits = FloatHelper.GetMantissaBits(position.X);
                string yMantissaBits = FloatHelper.GetMantissaBits(position.Y);
                messageLength = IntHelper.ParseBitString(FloatHelper.GetMantissaBitsWithMask(ref xMantissaBits, FrameHelper.FifteenBitsMantissaMask));
                this.position = IntHelper.ParseBitString(FloatHelper.GetMantissaBitsWithMask(ref yMantissaBits, FrameHelper.ThreeBitsMantissaMask));
                frameIndex++;
                return;
            }

            if (readString.Length < messageLength)
            {
                string xFraction = FloatHelper.GetFraction(ref position.X).PadRight(this.position + 1, '0');
                string yFraction = FloatHelper.GetFraction(ref position.Y).PadRight(this.position + 1, '0');
                string leftDigit = xFraction[this.position].ToString();
                string rightDigit = yFraction[this.position].ToString();
                int ascii = int.Parse($"{leftDigit}{rightDigit}");

                if (ascii < 95)
                {
                    char letter = (char)(ascii + 32);
                    readString += letter;
                }
            }

            frameIndex++;
        }

        public string GetDecodedMessage()
        {
            return readString;
        }

        public IDecoder Clone()
        {
            return (LetterMappingDecoder)MemberwiseClone();
        }
    }
}
