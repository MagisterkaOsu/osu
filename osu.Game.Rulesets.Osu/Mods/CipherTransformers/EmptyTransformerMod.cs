// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class EmptyTransformerMod : OsuModCipher
    {
        public override string Name => "Empty";
        public override string Acronym => "EM";
        public override LocalisableString Description => "Empty";

        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            return mousePosition;
        }

        public override string Decode(List<ReplayFrame> frames)
        {
            return "<no value>";
        }
    }
}
