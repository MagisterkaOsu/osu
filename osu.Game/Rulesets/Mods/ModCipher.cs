// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModCipher : Mod, ITransformsReplayRecorder, IDecodesReplay
    {
        public override string Name => "Cipher";
        public override string Acronym => "CIP";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => OsuIcon.PlayStyleKeyboard;
        public override double ScoreMultiplier => 1;

        [SettingSource("Plaintext", "Enter text to encode", 0, SettingControlType = typeof(SettingsTextBox))]
        public Bindable<string> PlaintextBindable { get; } = new Bindable<string>("Test String");

        public Func<Vector2, Vector2>? TransformMouseInputDelegate;
        public virtual Func<Vector2, bool, Vector2>? TransformMouseInput { get; set; }
        public virtual Func<List<ReplayFrame>, string>? DecodedString { get; set; }

        /// <summary>
        /// Denotes the key to represent the cipher used
        /// </summary>
        [JsonIgnore]
        public virtual string FirstFrameKey => "";

        protected int RandomFrameOffset = new Random().Next(300, 400);
        protected int FrameCounter = -1;
    }
}
