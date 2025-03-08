// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Cipher.Transformers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.Ciphers
{
    public class NetworkTestMod : OsuModCipher
    {
        public override string Name => "Network Test";
        public override string Acronym => "NT";
        public override LocalisableString Description => "Tests order of frames sent over network";
        public override IconUsage? Icon => FontAwesome.Solid.AddressBook;

        private readonly NetworkTestEncoder encoder = new NetworkTestEncoder();
        private readonly NetworkTestDecoder decoder = new NetworkTestDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            return encoder.Encode(mousePosition, pressedActions, ref Plaintext);
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
