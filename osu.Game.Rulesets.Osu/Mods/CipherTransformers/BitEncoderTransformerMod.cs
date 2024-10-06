// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class BitEncoderTransformerMod : OsuModCipher
    {
        public override string Name => "Bit Encoder";
        public override string Acronym => "BE";
        public override LocalisableString Description => "Encodes data into mantissa bits";
        public override IconUsage? Icon => FontAwesome.Solid.QuestionCircle;

        [SettingSource("Mask", "Message mask", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> Mask { get; } = new Bindable<int?>(0);

        private bool wroteFirstFrame;
        private Random random = new Random();

        public override Vector2 Transform(Vector2 mousePosition, bool pressedActions)
        {
            if (Mask.Value == null) return mousePosition;

            if (!wroteFirstFrame)
            {
                transformFirstFrame(ref mousePosition);
                wroteFirstFrame = true;
            }
            else if (!pressedActions)
                transformNthFrame(ref mousePosition);

            return mousePosition;
        }

        private void transformFirstFrame(ref Vector2 mousePosition)
        {
            // Write data length to x
            int plainTextLength = Plaintext.GetBits().Length;
            string plainTextLengthBinary = Convert.ToString(plainTextLength, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.X, plainTextLengthBinary);

            // Write mask to y
            string maskBinary = Convert.ToString((int)Mask.Value!, 2).PadLeft(23, '0');
            FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, maskBinary);
        }

        private void transformNthFrame(ref Vector2 mousePosition)
        {
            if (Mask.Value == null || Mask.Value == 0)
                return;

            bool bitsLeftToEncode = Plaintext.GetBits().Length > 0;

            if (bitsLeftToEncode)
            {
                bool toEncode = random.Next(2) != 0;

                if (toEncode)
                {
                    string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                    FloatHelper.SetNthMantissaBit(ref xMantissaBits, 0, '1');
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);

                    int mask1SAmount = IntHelper.GetAmountOf1SInMask((int)Mask.Value);

                    string messageBits = Plaintext.GetBits(mask1SAmount);

                    string yMantissaBits = FloatHelper.GetMantissaBits(mousePosition.Y);
                    FloatHelper.SetMantissaBitsWithMask(ref yMantissaBits, (int)Mask.Value, messageBits);
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.Y, yMantissaBits);
                }
                else
                {
                    string xMantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                    FloatHelper.SetNthMantissaBit(ref xMantissaBits, 0, '0');
                    FloatHelper.ReplaceMantissaBits(ref mousePosition.X, xMantissaBits);
                }
            }
            else
            {
                string mantissaBits = FloatHelper.GetMantissaBits(mousePosition.X);
                FloatHelper.SetNthMantissaBit(ref mantissaBits, 0, '0');
                FloatHelper.ReplaceMantissaBits(ref mousePosition.X, mantissaBits);
            }
        }
    }
}
