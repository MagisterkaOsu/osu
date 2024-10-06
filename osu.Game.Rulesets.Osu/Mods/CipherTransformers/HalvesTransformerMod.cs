// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using System.Linq;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class HalvesTransformerMod : OsuModCipher
    {
        public override string Name => "Halves";
        public override string Acronym => "HV";
        public override LocalisableString Description => ".5 - '1' ; .0 - '0'";

        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            if (!Plaintext.AreBitsLeft()) return mousePosition;

            char xBit = Plaintext.GetBit();
            char yBit = Plaintext.GetBit();

            Vector2 result = mousePosition;
            if (xBit == '1')
                FloatHelper.ReplaceFraction(ref result.X, "5");
            else
                FloatHelper.ReplaceFraction(ref result.X, "0");

            if (yBit == '1')
                FloatHelper.ReplaceFraction(ref result.Y, "5");
            else
                FloatHelper.ReplaceFraction(ref result.Y, "0");
            return result;
        }

        public override string Decode(List<ReplayFrame> frames)
        {
            List<OsuReplayFrame> replayFrames = frames.Cast<OsuReplayFrame>().ToList();
            return "";
        }
    }
}
