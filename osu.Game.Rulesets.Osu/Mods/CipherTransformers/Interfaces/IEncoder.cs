// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherHelpers
{
    public interface IEncoder
    {
        /// <summary>
        /// Encodes data into the given mouse position based on pressed actions.
        /// </summary>
        /// <param name="mousePosition">The current mouse position.</param>
        /// <param name="pressedActions">Indicates if actions are being pressed.</param>
        /// <returns>The transformed mouse position containing encoded data.</returns>
        Vector2 Encode(Vector2 mousePosition, bool pressedActions, ref InputHelper input);
    }
}
