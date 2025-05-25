// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osuTK;
using Cipher.Helpers;
using System.IO;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class ReplayRecorder<T> : ReplayRecorder, IKeyBindingHandler<T>
        where T : struct
    {
        private readonly Score target;

        private readonly List<T> pressedActions = new List<T>();

        private InputManager inputManager;

        /// <summary>
        /// The frame rate to record replays at.
        /// </summary>
        public int RecordFrameRate { get; set; } = 60;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        public Size? fullscreenSize { get; set; } = null!;

        protected ReplayRecorder(Score target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;

            Depth = float.MinValue;
        }

        protected ReplayRecorder(Score target, Size fullscreenSize)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;

            Depth = float.MinValue;

            this.fullscreenSize = fullscreenSize;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();
            recordFrame(false);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            recordFrame(false);
            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<T> e)
        {
            pressedActions.Add(e.Action);
            recordFrame(true);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<T> e)
        {
            pressedActions.Remove(e.Action);
            recordFrame(true);
        }

        private bool generatedData = false;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private void recordFrame(bool important)
        {
            var last = target.Replay.Frames.LastOrDefault();

            if (!important && last != null && Time.Current - last.Time < (1000d / RecordFrameRate) * Clock.Rate)
                return;

            /// ==========================
            if (!generatedData)
            {
                string containingFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "osu_replay_data");
                string outputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "osu_replay_data", $"{fullscreenSize.Value.Width}x{fullscreenSize.Value.Height}");

                // Check if folder exists and if not create it
                if (!Directory.Exists(containingFolderPath))
                {
                    Directory.CreateDirectory(containingFolderPath);
                }

                List<string> float_strings_x = new List<string>();
                List<string> bit_strings_x = new List<string>();
                List<string> float_strings_y = new List<string>();
                List<string> bit_strings_y = new List<string>();

                int screenWidth = fullscreenSize.Value.Width;
                int screenHeight = fullscreenSize.Value.Height;

                int currentX = 0;
                int currentY = 0;

                float minX = float.PositiveInfinity;
                float minY = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;
                float maxY = float.NegativeInfinity;

                int minScreenX = int.MaxValue;
                int maxScreenX = int.MinValue;
                int minScreenY = int.MaxValue;
                int maxScreenY = int.MinValue;

                for (currentX = 0; currentX < screenWidth; currentX++)
                {
                    Vector2 mousePos = new Vector2(currentX, currentY);
                    Vector2? nullableGamePos = ScreenSpaceToGamefield?.Invoke(mousePos);

                    if (nullableGamePos.HasValue)
                    {
                        Vector2 gamePos = nullableGamePos.Value; // Get the actual Vector2 value.

                        // In System.Numerics.Vector2, components are X and Y (uppercase).
                        if (gamePos.X >= 0 && gamePos.X < 512)
                        {
                            // Convert gamePos.X to a string and add it to the list.
                            // Using "R" (round-trip) format specifier is recommended for floats
                            // as it preserves precision if you intend to parse it back to a float later.
                            float_strings_x.Add(gamePos.X.ToString("R"));
                            string bitStringX = Cipher.Helpers.FloatHelper.GetFloatBits(gamePos.X);
                            bit_strings_x.Add(bitStringX);

                            if (gamePos.X < minX) {minX = gamePos.X; minScreenX = currentX;}
                            if (gamePos.X > maxX) {maxX = gamePos.X; maxScreenX = currentX;}
                        }
                    }
                }

                for (currentY = 0; currentY < screenHeight; currentY++)
                {
                    Vector2 mousePos = new Vector2(currentX, currentY);
                    Vector2? nullableGamePos = ScreenSpaceToGamefield?.Invoke(mousePos);

                    if (nullableGamePos.HasValue)
                    {
                        Vector2 gamePos = nullableGamePos.Value; // Get the actual Vector2 value.

                        // In System.Numerics.Vector2, components are X and Y (uppercase).
                        if (gamePos.Y >= 0 && gamePos.Y < 384)
                        {
                            // Convert gamePos.X to a string and add it to the list.
                            // Using "R" (round-trip) format specifier is recommended for floats
                            // as it preserves precision if you intend to parse it back to a float later.
                            float_strings_y.Add(gamePos.Y.ToString("R"));
                            string bitStringY = Cipher.Helpers.FloatHelper.GetFloatBits(gamePos.Y);
                            bit_strings_y.Add(bitStringY);

                            if (gamePos.Y < minY) { minY = gamePos.Y; minScreenY = currentY;}
                            if (gamePos.Y > maxY) { maxY = gamePos.Y; maxScreenY = currentY;}
                        }
                    }
                }

                File.WriteAllLines($"{outputFilePath} float x", float_strings_x);
                File.WriteAllLines($"{outputFilePath} float y", float_strings_y);
                File.WriteAllLines($"{outputFilePath} bits x", bit_strings_x);
                File.WriteAllLines($"{outputFilePath} bits y", bit_strings_y);

                Console.WriteLine("Data written to files successfully.");
                notificationOverlay?.Post(new SimpleNotification()
                {
                    Icon = FontAwesome.Solid.ExclamationCircle,
                    Text = $"Generated screen playfield data to {outputFilePath}.",

                });

                generatedData = true;
            }

            /// =================

            var position = ScreenSpaceToGamefield?.Invoke(inputManager.CurrentState.Mouse.Position) ?? inputManager.CurrentState.Mouse.Position;

            if (TransformMouseInput != null) position = TransformMouseInput.Invoke(position, pressedActions.Count > 0);

            var frame = HandleFrame(position, pressedActions, last);

            if (frame != null)
            {
                target.Replay.Frames.Add(frame);

                spectatorClient?.HandleFrame(frame);
            }
        }

        protected abstract ReplayFrame HandleFrame(Vector2 mousePosition, List<T> actions, ReplayFrame previousFrame);
    }

    public abstract partial class ReplayRecorder : Component
    {
        public Func<Vector2, Vector2> ScreenSpaceToGamefield;
        public Func<Vector2, bool, Vector2> TransformMouseInput;
    }
}
