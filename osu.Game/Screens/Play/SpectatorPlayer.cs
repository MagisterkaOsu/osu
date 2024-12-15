// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Cipher.Helpers;
using Cipher.Interfaces;
using Cipher.Transformers;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.Play
{
    public abstract partial class SpectatorPlayer : Player
    {
        [Resolved]
        protected SpectatorClient SpectatorClient { get; private set; } = null!;

        private readonly Score score;

        protected override bool CheckModsAllowFailure()
        {
            if (!allowFail) return false;
            return base.CheckModsAllowFailure();
        }

        private bool allowFail;

        protected SpectatorPlayer(Score score, PlayerConfiguration? configuration = null)
            : base(configuration)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new OsuSpriteText
            {
                Text = $"Watching {score.ScoreInfo.User.Username} playing live!",
                Font = OsuFont.Default.With(size: 30),
                Y = 100,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DrawableRuleset.FrameStableClock.WaitingOnFrames.BindValueChanged(waiting =>
            {
                if (GameplayClockContainer is MasterGameplayClockContainer master)
                {
                    if (master.UserPlaybackRate.Value > 1 && waiting.NewValue) master.UserPlaybackRate.Value = 1;
                }
            }, true);
        }

        /// <summary>
        /// Should be called when it is apparent that the player being spectated has failed.
        /// This will subsequently stop blocking the fail screen from displaying (usually done out of safety).
        /// </summary>
        public void AllowFail() => allowFail = true;

        protected override void StartGameplay()
        {
            base.StartGameplay();

            // Start gameplay along with the very first arrival frame (the latest one).
            score.Replay.Frames.Clear();
            SpectatorClient.OnNewFrames += userSentFrames;
            SpectatorClient.OnNewFrames += tryFindDecoder;
        }

        private Dictionary<string, IDecoder> decoders = new Dictionary<string, IDecoder> { { BitEncoder.FIRST_FRAME_KEY, new BitDecoder() }, { HalvesEncoder.FIRST_FRAME_KEY, new HalvesDecoder() } };
        private IDecoder? decoder;

        private void tryFindDecoder(int userId, FrameDataBundle bundle)
        {
            if (decoder == null)
            {
                Console.WriteLine($"Trying to find decoder in {bundle.Frames.Count} new frames");

                foreach (var frame in bundle.Frames)
                {
                    string xBits = FloatHelper.GetFloatBits(frame.Position.X);
                    string yBits = FloatHelper.GetFloatBits(frame.Position.Y);
                    string frameKey = xBits + yBits;

                    if (decoders.TryGetValue(frameKey, out var value))
                    {
                        var matchingDecoder = value;
                        decoder = matchingDecoder;
                        Console.WriteLine("Found decoder");
                        SpectatorClient.OnNewFrames -= tryFindDecoder;
                        SpectatorClient.OnNewFrames += readReplayFrames;
                    }
                }
            }
        }

        private bool alreadyReading;

        private void readReplayFrames(int userId, FrameDataBundle bundle)
        {
            if (decoder != null && !alreadyReading)
            {
                Console.WriteLine("Reading replay frames for messages");
                alreadyReading = true;
                var clonedDecoder = decoder.Clone();

                foreach (var frame in Score.Replay.Frames)
                {
                    clonedDecoder.ProcessFrame(frame);
                }

                string currentResult = clonedDecoder.GetDecodedMessage();
                Console.WriteLine($"Current message: {currentResult}");

                alreadyReading = false;
            }
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            if (userId != score.ScoreInfo.User.OnlineID) return;
            if (!LoadedBeatmapSuccessfully) return;
            if (!this.IsCurrentScreen()) return;
            bool isFirstBundle = score.Replay.Frames.Count == 0;

            foreach (var frame in bundle.Frames)
            {
                IConvertibleReplayFrame convertibleFrame = GameplayState.Ruleset.CreateConvertibleReplayFrame()!;
                convertibleFrame.FromLegacy(frame, GameplayState.Beatmap);
                var convertedFrame = (ReplayFrame)convertibleFrame;
                convertedFrame.Time = frame.Time;
                convertedFrame.Header = frame.Header;
                score.Replay.Frames.Add(convertedFrame);
            }

            if (isFirstBundle && score.Replay.Frames.Count > 0) SetGameplayStartTime(score.Replay.Frames[0].Time);
        }

        protected override Score CreateScore(IBeatmap beatmap) => score;
        protected override ResultsScreen CreateResults(ScoreInfo score) => new SpectatorResultsScreen(score);

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(score);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            SpectatorClient.OnNewFrames -= userSentFrames;
            return base.OnExiting(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SpectatorClient.IsNotNull())
            {
                SpectatorClient.OnNewFrames -= userSentFrames;
                SpectatorClient.OnNewFrames -= tryFindDecoder;
                SpectatorClient.OnNewFrames -= readReplayFrames;
            }
        }
    }
}
