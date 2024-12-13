// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Replays;

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public interface IDecoder
    {
        /// <summary>
        /// Processes a single replay frame to extract encoded data.
        /// </summary>
        /// <param name="frame">The replay frame to process.</param>
        void ProcessFrame(OsuReplayFrame frame);

        /// <summary>
        /// Retrieves the decoded message after processing frames.
        /// </summary>
        /// <returns>The decoded message as a string.</returns>
        string GetDecodedMessage();
    }
}
