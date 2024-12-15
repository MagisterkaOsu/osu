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

        protected bool WroteFirstFrame;

        protected void TransformFirstFrame(ref Vector2 mousePosition)
        {
            if (WroteFirstFrame) throw new ArgumentException("Trying to write first frame when it was already written");
            if (FirstFrameKey.Length != 64) throw new ArgumentException("FirstFrameKey is of invalid length");

            // Split FirstFrameKey into two parts
            string xBits = FirstFrameKey.Substring(0, 32);
            string yBits = FirstFrameKey.Substring(32, 32);

            // Write the parts to the mantissas of X and Y
            FloatHelper.ReplaceBits(ref mousePosition.X, xBits);
            FloatHelper.ReplaceBits(ref mousePosition.Y, yBits);
            WroteFirstFrame = true;
        }
    }
}
