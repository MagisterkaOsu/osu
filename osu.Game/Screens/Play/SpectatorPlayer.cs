// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cipher.Helpers;
using Cipher.Interfaces;
using Cipher.Transformers;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        #region Cipher Decoding

        /// <summary>
        /// All decoders that can be used to decode the frames.
        /// </summary>
        private readonly Dictionary<string, IDecoder> decoders = new Dictionary<string, IDecoder>
        {
            { LSBMaskEncoder.FIRST_FRAME_KEY, new LSBMaskDecoder() },
            { FractionsEncoder.FIRST_FRAME_KEY, new FractionsDecoder() },
            { NetworkTestEncoder.FIRST_FRAME_KEY, new NetworkTestDecoder() },
            { LetterMappingEncoder.FIRST_FRAME_KEY, new LetterMappingDecoder() },
            { DecimalPositionEncoder.FIRST_FRAME_KEY, new DecimalPositionDecoder() }
        };

        /// <summary>
        /// Currently used decoder.
        /// </summary>
        private IDecoder? decoder;

        /// <summary>
        /// Queue for frames to be later decoding when decoder is found.
        /// </summary>
        private readonly Queue<object> decodingQueue = new Queue<object>();

        /// <summary>
        /// Token source for cancelling the decoding task.
        /// </summary>
        private CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// How long to wait for new frames.
        /// </summary>
        private const int decoder_polling_interval = 1000;

        public Bindable<string> CurrentMessage = new Bindable<string>();

        private void tryFindDecoder(int userId, FrameDataBundle bundle)
        {
            if (decoder == null)
            {
                Console.WriteLine($"Trying to find decoder in {bundle.Frames.Count} new replay frames");

                foreach (var frame in bundle.Frames)
                {
                    var position = frame.Position;
                    string frameKey = FrameHelper.GetPotentialFirstFrameKey(ref position);

                    if (decoders.TryGetValue(frameKey, out var value))
                    {
                        var matchingDecoder = value;
                        decoder = matchingDecoder;
                        Console.WriteLine($"Found decoder: {decoder.GetType().Name}");

                        SpectatorClient.OnNewFrames -= tryFindDecoder;
                        _ = sendFramesToDecoder(cts.Token);
                    }
                }
            }
        }

        /// <summary>
        /// Task for sending frames to the decoder.
        /// </summary>
        private async Task sendFramesToDecoder(CancellationToken token)
        {
            if (decoder != null)
            {
                while (!token.IsCancellationRequested)
                {
                    if (decodingQueue.TryDequeue(out object frame))
                    {
                        decoder.ProcessFrame(frame);
                    }
                    else
                    {
                        string message = decoder.GetDecodedMessage();
                        CurrentMessage.Value = message;
                        await Task.Delay(decoder_polling_interval, token).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion

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
                decodingQueue.Enqueue(convertedFrame);
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
                decodingQueue.Clear();
                cts.Cancel();
            }
        }
    }
}
