// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Cipher.Helpers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public abstract class OsuModCipher : ModCipher, IApplicableToBeatmap
    {
        public override IconUsage? Icon => null;
        public override ModType Type => ModType.Ciphers;
        public override Type[] IncompatibleMods => [];
        public InputHelper Plaintext = null!;

        public override Func<Vector2, bool, Vector2>? TransformMouseInput
        {
            get => Transform;
            set => base.TransformMouseInput = value;
        }

        public override Func<List<ReplayFrame>, string>? DecodedString
        {
            get => Decode;
            set => base.DecodedString = value;
        }

        /// <summary>
        /// Runs every replay frame used by ReplayRecorder
        /// </summary>
        public abstract Vector2 Transform(Vector2 mousePosition, bool pressedActions);

        /// <summary>
        /// Runs once at the start of replay
        /// </summary>
        public abstract string Decode(List<ReplayFrame> frames);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            Plaintext = new InputHelper(PlaintextBindable.Value);
        }
    }
}
