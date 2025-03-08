// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;
using Cipher.Interfaces;
using osuTK;

namespace Cipher.Transformers
{
    public class NetworkTestEncoder : IEncoder
    {
        public static string FIRST_FRAME_KEY { get; } = "1011111110010101100111110111100010111111111100101010101101111010";
        private bool wroteFirstFrame;
        private int frameCount;

        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters)
        {
            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
                return mousePosition;
            }

            transformNthFrame(ref mousePosition);
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

        private void transformNthFrame(ref Vector2 mousePosition)
        {
            FloatHelper.ReplaceFraction(ref mousePosition.X, $"{frameCount++}");
        }
    }

    public class NetworkTestDecoder : IDecoder
    {
        private string readBits = string.Empty;
        private int frameIndex;
        private List<int> messagesIndexes = new List<int>();

        public void ProcessFrame(object frame)
        {
            var fieldInfo = frame.GetType().GetField("Position");
            Vector2 position = (Vector2)fieldInfo.GetValue(frame);

            if (frameIndex == 0)
            {
                string xBits = FloatHelper.GetFloatBits(position.X);
                string yBits = FloatHelper.GetFloatBits(position.Y);
                string frameKey = xBits + yBits;

                if (frameKey == NetworkTestEncoder.FIRST_FRAME_KEY)
                {
                    frameIndex++;
                }

                return;
            }

            string xFraction = FloatHelper.GetFraction(ref position.X);

            if (!int.TryParse(xFraction, out int messageIndex))
            {
                return;
            }

            messagesIndexes.Add(messageIndex);
        }

        public string GetDecodedMessage()
        {
            return string.Join(" ", messagesIndexes);
        }

        public IDecoder Clone()
        {
            return (NetworkTestDecoder)MemberwiseClone();
        }
    }
}
