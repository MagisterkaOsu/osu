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
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.Ciphers
{
    public class DecimalPositionEncoderMod : OsuModCipher
    {
        public override string Name => "Decimal Position";
        public override string Acronym => "DP";
        public override LocalisableString Description => "Replaces specific digit in fraction";
        public override IconUsage? Icon => FontAwesome.Solid.Calculator;

        [SettingSource("Position", "Position of replaced digit", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Position { get; } = new Bindable<int?>(0);

        public override Type[] IncompatibleMods => new[] { typeof(LSBMaskEncoderMod), typeof(FractionsTransformerMod), typeof(LetterMappingMod) };

        private readonly DecimalPositionEncoder encoder = new DecimalPositionEncoder();
        private readonly DecimalPositionDecoder decoder = new DecimalPositionDecoder();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            FrameCounter++;
            return FrameCounter > RandomFrameOffset ? encoder.Encode(mousePosition, pressedActions, ref Plaintext, Position.Value) : mousePosition;
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
