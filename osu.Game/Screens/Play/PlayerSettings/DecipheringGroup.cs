// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class DecipheringGroup : PlayerSettingsGroup
    {
        [SettingSource("Decoded string", SettingControlType = typeof(SettingsSpriteTextArea))]
        public Bindable<string> DecodedString { get; } = new Bindable<string>
        {
            Default = string.Empty,
        };

        public DecipheringGroup()
            : base("Deciphering")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(this.CreateSettingsControls());
        }
    }
}
