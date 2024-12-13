// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using System.Linq;
using Cipher.Transformers;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class HalvesTransformerMod : OsuModCipher
    {
        public override string Name => "Halves";
        public override string Acronym => "HV";
        public override LocalisableString Description => ".5 - '1' ; .0 - '0'";
        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;
        public override string FirstFrameKey => HalvesEncoder.FIRST_FRAME_KEY;

        private readonly HalvesEncoder encoder = new HalvesEncoder();
        private readonly HalvesDecoder decoder = new HalvesDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            FrameCounter++;
            return FrameCounter > RandomFrameOffset ? encoder.Encode(mousePosition, pressedActions, ref Plaintext) : mousePosition;
        }

        public override string Decode(List<ReplayFrame> frames)
        {
            foreach (var frame in frames.Cast<OsuReplayFrame>())
            {
                decoder.ProcessFrame(frame);
            }

            return decoder.GetDecodedMessage();
        }
    }
}
