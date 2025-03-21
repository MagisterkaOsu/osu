// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class DecimalPositionEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "001110001000110000110001101111";
        private bool wroteFirstFrame;
        private bool wroteSecondFrame;
        private readonly Random random = new Random();
        public static readonly int[] BIT0_POOL = { 1, 4, 7 };
        public static readonly int[] BIT1_POOL = { 2, 5, 8 };
        public static readonly int[] NO_MSG_POOL = { 0, 3, 6, 9 };

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters)
        {
            int? position = null;

            if (parameters != null && parameters.Length > 0 && parameters[0] is int?)
            {
                position = (int?)parameters[0];
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
                if (!pressedActions)
                {
                    bool toEncodeX = random.Next(99) < 60;
                    bool toEncodeY = random.Next(99) < 60;
                    int xDecimalValue, yDecimalValue;
                    string xFractionalPart = FloatHelper.GetFraction(ref mousePosition.X).PadRight(position + 1, '0');
                    string yFractionalPart = FloatHelper.GetFraction(ref mousePosition.Y).PadRight(position + 1, '0');
                    int xDigit = int.Parse(xFractionalPart[position].ToString());
                    int yDigit = int.Parse(yFractionalPart[position].ToString());

                    if (toEncodeX)
                    {
                        char xMessageBit = input.GetBit();
                        xDecimalValue = pickClosestValueFromPool(xMessageBit == '1' ? BIT1_POOL : BIT0_POOL, xDigit);
                    }
                    else
                    {
                        xDecimalValue = pickClosestValueFromPool(NO_MSG_POOL, xDigit);
                    }

                    if (toEncodeY)
                    {
                        char yMessageBit = input.GetBit();
                        yDecimalValue = pickClosestValueFromPool(yMessageBit == '1' ? BIT1_POOL : BIT0_POOL, yDigit);
                    }
                    else
                    {
                        yDecimalValue = pickClosestValueFromPool(NO_MSG_POOL, yDigit);
                    }

                    xFractionalPart = xFractionalPart.Remove(position, 1).Insert(position, xDecimalValue.ToString());
                    yFractionalPart = yFractionalPart.Remove(position, 1).Insert(position, yDecimalValue.ToString());
                    FloatHelper.ReplaceFraction(ref mousePosition.X, xFractionalPart);
                    FloatHelper.ReplaceFraction(ref mousePosition.Y, yFractionalPart);
                }
            }
        }

        private int pickClosestValueFromPool(int[] pool, int prevValue)
        {
            int closestValue = pool[0];
            int closestDistance = int.MaxValue;

            foreach (int value in pool)
            {
                int distance = value - prevValue;

                if (distance < 0)
                {
                    distance = -distance;
                }

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestValue = value;
                }
            }

            return closestValue;
        }
    }

    public class DecimalPositionDecoder : IDecoder
    {
        public string readBits = string.Empty;
        private int messageLength;
        private int position;
        private int frameIndex;

        public void ProcessFrame(object frame)
        {
            Vector2 position = FrameHelper.GetPositionFromFrameObject(ref frame);
            IList actions = FrameHelper.GetActionsFromFrameObject(ref frame);

            if (frameIndex == 0)
            {
                string frameKey = FrameHelper.GetPotentialFirstFrameKey(ref position);

                if (frameKey == DecimalPositionEncoder.FIRST_FRAME_KEY)
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
                if (actions.Count > 0) return;
                string xFractionalPart = FloatHelper.GetFraction(ref position.X).PadRight(this.position + 1, '0');
                string yFractionalPart = FloatHelper.GetFraction(ref position.Y).PadRight(this.position + 1, '0');
                int xDigit = int.Parse(xFractionalPart[this.position].ToString());
                int yDigit = int.Parse(yFractionalPart[this.position].ToString());

                if (Array.Exists(DecimalPositionEncoder.BIT1_POOL, element => element == xDigit))
                {
                    readBits += '1';
                }
                else if (Array.Exists(DecimalPositionEncoder.BIT0_POOL, element => element == xDigit))
                {
                    readBits += '0';
                }

                if (Array.Exists(DecimalPositionEncoder.BIT1_POOL, element => element == yDigit))
                {
                    readBits += '1';
                }
                else if (Array.Exists(DecimalPositionEncoder.BIT0_POOL, element => element == yDigit))
                {
                    readBits += '0';
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
            return (DecimalPositionDecoder)MemberwiseClone();
        }
    }
}
