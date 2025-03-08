// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Cipher.Helpers;
using osuTK;

namespace Cipher.Interfaces
{
    public interface IEncoder
    {
        /// <summary>
        /// Key used to encode the first frame of a replay.
        /// Used for finding the correct encoder/decoder pair.
        /// </summary>
        public static string FIRST_FRAME_KEY { get; }

        /// <summary>
        /// Encodes a replay frame.
        /// </summary>
        /// <param name="mousePosition">Current mouse position</param>
        /// <param name="pressedActions">Is any mouse button pressed</param>
        /// <param name="input">Used to read plaintext message</param>
        /// <param name="parameters">Additional parameters required by specific encoder</param>
        /// <returns></returns>
        public Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input, params object[] parameters);
    }
}
