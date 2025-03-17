// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayShowCoords : PlayerSettingsGroup
    {
        private readonly OsuRulesetConfigManager config;

        // BindableNumber<float> creates a slider, but SettingsNumberBox only accepts an int. That's why a textBox needs to be used.

        [SettingSource("Decoded string", SettingControlType = typeof(SettingsSpriteTextArea))]
        public Bindable<string> DecodedString { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        [SettingSource("X (float)", SettingControlType = typeof(SettingsSpriteText))]
        public Bindable<string> ReplayPlayerX { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        [SettingSource("X (binary)", SettingControlType = typeof(SettingsSpriteTextFixed))]
        public Bindable<string> ReplayPlayerXBinary { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        [SettingSource("Y (float)", SettingControlType = typeof(SettingsSpriteText))]
        public Bindable<string> ReplayPlayerY { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        [SettingSource("Y (binary)", SettingControlType = typeof(SettingsSpriteTextFixed))]
        public Bindable<string> ReplayPlayerYBinary { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        public ReplayShowCoords(OsuRulesetConfigManager config)
            : base("Deciphering and position")
        {
            this.config = config;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(this.CreateSettingsControls());

            config.BindWith(OsuRulesetSetting.DecodedString, DecodedString);
            config.BindWith(OsuRulesetSetting.ReplayPlayerX, ReplayPlayerX);
            config.BindWith(OsuRulesetSetting.ReplayPlayerY, ReplayPlayerY);
            config.BindWith(OsuRulesetSetting.ReplayPlayerXBinary, ReplayPlayerXBinary);
            config.BindWith(OsuRulesetSetting.ReplayPlayerYBinary, ReplayPlayerYBinary);
        }
    }
}
