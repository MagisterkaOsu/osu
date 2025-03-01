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
    public class LetterMappingMod : OsuModCipher
    {
        public override string Name => "Letter Mapping";
        public override string Acronym => "LM";
        public override LocalisableString Description => "Maps letters to cursor positions";
        public override IconUsage? Icon => FontAwesome.Solid.AddressBook;

        [SettingSource("Position", "Fractional position", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Position { get; } = new Bindable<int?>(0);
        public override Type[] IncompatibleMods => new[] { typeof(FractionsTransformerMod) };

        private readonly LetterMappingEncoder encoder = new LetterMappingEncoder();
        private readonly LetterMappingDecoder decoder = new LetterMappingDecoder();

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
