// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osu.Game.Rulesets.Osu.Mods.CipherTransformers;
using osu.Game.Rulesets.Osu.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayReadCoords : CompositeDrawable
    {
        private Bindable<string> decodedString { get; set; } = new Bindable<string>("");
        private Bindable<string> replayPlayerX { get; set; } = new Bindable<string>("0");
        private Bindable<string> replayPlayerXBinary { get; set; } = new Bindable<string>("0b0");
        private Bindable<string> replayPlayerY { get; set; } = new Bindable<string>("0");
        private Bindable<string> replayPlayerYBinary { get; set; } = new Bindable<string>("0b0");

        private readonly Replay replay;
        private readonly List<OsuReplayFrame> replayFrames;
        private readonly IReadOnlyList<Mod> mods;
        private int currentFrame = -1;
        private double lastTime = double.MinValue;

        public ReplayReadCoords(Replay replay, IReadOnlyList<Mod> mods = null)
        {
            RelativeSizeAxes = Axes.Both;
            this.replay = replay;
            replayFrames = replay.Frames.Cast<OsuReplayFrame>().ToList();
            this.mods = mods?.ToArray() ?? Array.Empty<Mod>();
        }

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager config)
        {
            config.BindWith(OsuRulesetSetting.DecodedString, decodedString);
            config.BindWith(OsuRulesetSetting.ReplayPlayerX, replayPlayerX);
            config.BindWith(OsuRulesetSetting.ReplayPlayerXBinary, replayPlayerXBinary);
            config.BindWith(OsuRulesetSetting.ReplayPlayerY, replayPlayerY);
            config.BindWith(OsuRulesetSetting.ReplayPlayerYBinary, replayPlayerYBinary);
        }

        protected override void LoadComplete()
        {
            tryDecodeReplay();
        }

        private void tryDecodeReplay()
        {
            var firstFrame = replayFrames.FirstOrDefault();
            if (firstFrame == null) return;

            string xBits = FloatHelper.GetFloatBits(firstFrame.Position.X);
            string yBits = FloatHelper.GetFloatBits(firstFrame.Position.Y);
            string frameKey = xBits + yBits;

            var cipherModsInstances = new List<ModCipher>
            {
                new BitEncoderTransformerMod(),
                new HalvesTransformerMod(),
            };

            switch (frameKey)
            {
                case var key when cipherModsInstances.Any(instance => instance.FirstFrameKey == key):
                    var matchingMod = cipherModsInstances.First(instance => instance.FirstFrameKey == frameKey);
                    decodedString.Value = matchingMod.DecodedString?.Invoke(replay.Frames) ?? "<no value>";
                    break;

                default:
                    decodedString.Value = new EmptyTransformerMod().DecodedString?.Invoke(replay.Frames) ?? "<no value>";
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (currentFrame == replayFrames.Count - 1) return; //No more frames

            double time = Clock.CurrentTime;
            bool changeCoords = false;

            // support for next and last frame - Check if the time has changed
            if (lastTime != time)
            {
                // Determine the next frame based on whether time is moving forward or backward
                int nextFrame = currentFrame + (lastTime < time ? 1 : -1);

                lastTime = time;

                // currentFrame < 0 - first frame; set initial value
                // nextFrame >= 0 - to avoid negative indices
                // check if time from next frame is closer to time from current frame
                if (currentFrame < 0 || (nextFrame >= 0 && Math.Abs(replayFrames[nextFrame].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time)))
                {
                    currentFrame = nextFrame;
                    changeCoords = true; // Indicate that shown replay coordinates need to be updated
                }
            }

            if (changeCoords)
            {
                Vector2 position = replayFrames[currentFrame].Position;
                replayPlayerX.Value = position.X.ToString(CultureInfo.InvariantCulture);
                replayPlayerY.Value = position.Y.ToString(CultureInfo.InvariantCulture);
                replayPlayerXBinary.Value = getBitRepresentationOfPosition(position.X);
                replayPlayerYBinary.Value = getBitRepresentationOfPosition(position.Y);
            }
        }

        private static string getBitRepresentationOfPosition(float position)
        {
            string bitRepresentation = Convert.ToString(BitConverter.SingleToInt32Bits(position), 2).PadLeft(32, '0');
            return $"0b{bitRepresentation}";
        }
    }
}
