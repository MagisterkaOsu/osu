// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using System.Linq;
using Cipher.Transformers;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.Ciphers
{
    public class FractionsTransformerMod : OsuModCipher
    {
        public override string Name => "Fractions";
        public override string Acronym => "FRC";
        public override LocalisableString Description => ".5 - '1' ; .0 - '0'";
        public override IconUsage? Icon => FontAwesome.Solid.StarHalf;
        public override Type[] IncompatibleMods => new[] { typeof(LSBMaskEncoderMod) };

        private readonly FractionsEncoder encoder = new FractionsEncoder();
        private readonly FractionsDecoder decoder = new FractionsDecoder();

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
