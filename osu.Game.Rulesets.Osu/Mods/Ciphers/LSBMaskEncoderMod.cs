// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

namespace osu.Game.Rulesets.Osu.Mods.Ciphers
{
    public class LSBMaskEncoderMod : OsuModCipher
    {
        public override string Name => "LSB Mask";
        public override string Acronym => "LSB";
        public override LocalisableString Description => "Masks mantissa bits in cursor data";
        public override IconUsage? Icon => FontAwesome.Solid.LayerGroup;

        [SettingSource("Mask", "Message mask", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Mask { get; } = new Bindable<int?>(0);
        public override Type[] IncompatibleMods => new[] { typeof(FractionsTransformerMod), typeof(LetterMappingMod), typeof(DecimalPositionEncoderMod) };

        private readonly LSBMaskEncoder encoder = new LSBMaskEncoder();
        private readonly LSBMaskDecoder decoder = new LSBMaskDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            FrameCounter++;
            return FrameCounter > RandomFrameOffset ? encoder.Encode(mousePosition, pressedActions, ref Plaintext, Mask.Value) : mousePosition;
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
