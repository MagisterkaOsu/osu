// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using osuTK;

namespace Cipher.Helpers
{
    public class FrameHelper
    {
        public static int FifteenBitsMantissaMask { get; } = 32767;

        public static void TransformFirstFrame(ref Vector2 mousePosition, string firstFrameKey)
        {
            if (firstFrameKey.Length != 30)
            {
                throw new ArgumentException("FirstFrameKey must be 30 characters long", nameof(firstFrameKey));
            }

            string xBits = firstFrameKey.Substring(0, 15);
            string yBits = firstFrameKey.Substring(15, 15);

            string xMantissa = FloatHelper.GetMantissaBits(mousePosition.X);
            string yMantissa = FloatHelper.GetMantissaBits(mousePosition.Y);

            FloatHelper.SetMantissaBitsWithMask(ref xMantissa, FifteenBitsMantissaMask, xBits);
            FloatHelper.SetMantissaBitsWithMask(ref yMantissa, FifteenBitsMantissaMask, yBits);

            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissa);
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, yMantissa);
        }

        public static string GetPotentialFirstFrameKey(ref Vector2 mousePosition)
        {
            string xMantissa = FloatHelper.GetMantissaBits(mousePosition.X);
            string yMantissa = FloatHelper.GetMantissaBits(mousePosition.Y);

            string xBits = FloatHelper.GetMantissaBitsWithMask(ref xMantissa, FifteenBitsMantissaMask);
            string yBits = FloatHelper.GetMantissaBitsWithMask(ref yMantissa, FifteenBitsMantissaMask);
            return xBits + yBits;
        }

        public static Vector2 GetPositionFromFrameObject(ref object frame)
        {
            var fieldInfo = frame.GetType().GetField("Position");
            Vector2 position = (Vector2)fieldInfo.GetValue(frame);
            return position;
        }

        public static IList GetActionsFromFrameObject(ref object frame)
        {
            var fieldInfo = frame.GetType().GetField("Actions");
            var actions = fieldInfo.GetValue(frame) as System.Collections.IList;
            return actions;
        }

        public static bool IsFrameGood(ref object frame)
        {
            Vector2 position = GetPositionFromFrameObject(ref frame);
            IList actions = GetActionsFromFrameObject(ref frame);
            return actions.Count == 0 && position.X >= 0 && position.Y >= 0 && position.X < 512 && position.Y < 384;
        }

        public static bool IsFrameGood(Vector2 position, IList actions)
        {
            return actions.Count == 0 && position.X >= 0 && position.Y >= 0 && position.X < 512 && position.Y < 384;
        }

        public static bool IsFrameGood(Vector2 position, bool pressedActions)
        {
            return !pressedActions && position.X >= 0 && position.Y >= 0 && position.X < 512 && position.Y < 384;
        }
    }
}
