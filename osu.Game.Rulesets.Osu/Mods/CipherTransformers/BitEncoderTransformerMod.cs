// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Cipher.Transformers;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class BitEncoderTransformerMod : OsuModCipher
    {
        public override string Name => "Bit Encoder";
        public override string Acronym => "BE";
        public override LocalisableString Description => "Puts data in mantissa bits of cursor";
        public override IconUsage? Icon => FontAwesome.Solid.Columns;

        [SettingSource("Mask", "Message mask", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Mask { get; } = new Bindable<int?>(0);

        public override string FirstFrameKey => BitEncoder.FIRST_FRAME_KEY;

        private readonly BitEncoder encoder = new BitEncoder();
        private readonly BitDecoder decoder = new BitDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            return encoder.Encode(mousePosition, pressedActions, ref Plaintext, Mask.Value);
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
