// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsSpriteText : SettingsItem<string>
    {
        public SettingsSpriteText()
        {
            SpriteText valueText;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            labelText = new OsuSpriteText(),
                            valueText = new OsuSpriteText(OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold)),
                        }
                    }
                }
            };

            valueText.Colour = Colour4.White;
            valueText.Current.BindTo(Current);
            valueText.Current.BindValueChanged(curr =>
            {
                if (curr.NewValue == "<no message>")
                {
                    valueText.Colour = Colour4.MediumVioletRed;
                }
                else
                {
                    valueText.Colour = Colour4.White;
                }
            });
        }

        private readonly SpriteText labelText;

        protected override Drawable CreateControl() => new OsuSpriteText();

        public override LocalisableString LabelText
        {
            get => labelText.Text;
            set => labelText.Text = value;
        }

        public override Bindable<string> Current
        {
            get => base.Current;
            set
            {
                if (value.Default == null)
                    throw new InvalidOperationException($"Bindable settings of type {nameof(Bindable<string>)} should have a non-null default value.");

                base.Current = value;
            }
        }
    }
}
