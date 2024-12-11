// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods.CipherHelpers;
using osu.Game.Rulesets.Osu.Mods.CipherTransformers;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods.CipherMods
{
    [TestFixture]
    public class BitEncoderTransformerModTest
    {
        private BitEncoderTransformerMod mod = new BitEncoderTransformerMod();

        [Test]
        public void TestDecode()
        {
            List<OsuReplayFrame> frames =
            [
                new OsuReplayFrame(-1947, new Vector2(-146.49411f, -56.0006f)), new OsuReplayFrame(17, new Vector2(256.0009765625f, 128.00013732910156f)),
                new OsuReplayFrame(16, new Vector2(261.5682373046875f, 137.88364f)), new OsuReplayFrame(17, new Vector2(261.5682373046875f, 137.88364f)),
                new OsuReplayFrame(17, new Vector2(261.5682373046875f, 137.88364f)), new OsuReplayFrame(16, new Vector2(261.5682373046875f, 137.88364f)),
                new OsuReplayFrame(17, new Vector2(261.5682678222656f, 137.88365173339844f)), new OsuReplayFrame(17, new Vector2(261.5682373046875f, 137.88364f)),
                new OsuReplayFrame(16, new Vector2(261.5682373046875f, 137.88364f)), new OsuReplayFrame(17, new Vector2(261.5682678222656f, 137.88365173339844f)),
                new OsuReplayFrame(17, new Vector2(261.5682678222656f, 137.88365173339844f)), new OsuReplayFrame(16, new Vector2(261.5682373046875f, 137.88364f)),
                new OsuReplayFrame(13, new Vector2(262.04278564453125f, 138.68794f)), new OsuReplayFrame(17, new Vector2(273.29962158203125f, 196.13869f)),
                new OsuReplayFrame(16, new Vector2(273.29962158203125f, 196.13869f)), new OsuReplayFrame(17, new Vector2(273.29962158203125f, 196.13869f)),
                new OsuReplayFrame(12, new Vector2(273.3079833984375f, 196.15286f)), new OsuReplayFrame(13, new Vector2(291.67974853515625f, 179.21819f)),
                new OsuReplayFrame(20, new Vector2(293.87628173828125f, 167.4521f)), new OsuReplayFrame(18, new Vector2(279.16143798828125f, 154.22101f)),
                new OsuReplayFrame(18, new Vector2(259.30841064453125f, 146.13737f)), new OsuReplayFrame(19, new Vector2(250.4864501953125f, 143.93483f)),
                new OsuReplayFrame(7, new Vector2(247.5460968017578f, 143.93560791015625f)), new OsuReplayFrame(17, new Vector2(237.2550048828125f, 146.14241f)),
                new OsuReplayFrame(18, new Vector2(229.90463256835938f, 152.75902f)), new OsuReplayFrame(18, new Vector2(226.96453857421875f, 160.11008f)),
                new OsuReplayFrame(18, new Vector2(226.22967529296875f, 165.99078f)), new OsuReplayFrame(17, new Vector2(228.43496704101562f, 170.40123f)),
                new OsuReplayFrame(18, new Vector2(234.31552124023438f, 172.60643f)), new OsuReplayFrame(18, new Vector2(240.93109130859375f, 174.07658f)),
                new OsuReplayFrame(1, new Vector2(240.93109130859375f, 174.81166f)), new OsuReplayFrame(17, new Vector2(246.8116455078125f, 173.34152f)),
                new OsuReplayFrame(18, new Vector2(251.22207641601562f, 165.25575f)), new OsuReplayFrame(1, new Vector2(251.22207641601562f, 165.25575f)),
                new OsuReplayFrame(18, new Vector2(251.95712280273438f, 155.69986f)), new OsuReplayFrame(18, new Vector2(246.07659912109375f, 149.08424f)),
                new OsuReplayFrame(18, new Vector2(234.31549072265625f, 143.93877f)), new OsuReplayFrame(16, new Vector2(228.4349365234375f, 143.93877f)),
                new OsuReplayFrame(14, new Vector2(220.34918212890625f, 147.6141f)), new OsuReplayFrame(20, new Vector2(207.117919921875f, 165.25575f)),
                new OsuReplayFrame(18, new Vector2(202.70751953125f, 183.63248f)), new OsuReplayFrame(19, new Vector2(209.32315063476562f, 196.86372f)),
                new OsuReplayFrame(19, new Vector2(230.64013671875f, 204.94948f)), new OsuReplayFrame(19, new Vector2(253.42727661132812f, 205.68456f)),
                new OsuReplayFrame(5, new Vector2(258.57275390625f, 203.47935f)), new OsuReplayFrame(17, new Vector2(271.803955078125f, 191.71825f)),
                new OsuReplayFrame(17, new Vector2(279.1546630859375f, 178.487f)), new OsuReplayFrame(4, new Vector2(279.88970947265625f, 174.81166f)),
                new OsuReplayFrame(19, new Vector2(274.00921630859375f, 160.11028f)), new OsuReplayFrame(17, new Vector2(257.10260009765625f, 150.55438f)),
                new OsuReplayFrame(20, new Vector2(226.229736328125f, 141.73355f)), new OsuReplayFrame(10, new Vector2(213.7335662841797f, 143.2036895751953f)),
                new OsuReplayFrame(18, new Vector2(202.70753479003906f, 157.17010498046875f)), new OsuReplayFrame(19, new Vector2(189.47628784179688f, 178.487f)),
                new OsuReplayFrame(18, new Vector2(181.3905487060547f, 202.74415588378906f)), new OsuReplayFrame(17, new Vector2(181.3905487060547f, 218.1807098388672f)),
                new OsuReplayFrame(17, new Vector2(189.47630310058594f, 226.26637268066406f)), new OsuReplayFrame(17, new Vector2(201.23736572265625f, 229.94182f)),
                new OsuReplayFrame(18, new Vector2(214.46864318847656f, 232.1471405029297f)), new OsuReplayFrame(17, new Vector2(226.96478271484375f, 230.6769f)),
                new OsuReplayFrame(19, new Vector2(235.050537109375f, 225.53142f)), new OsuReplayFrame(18, new Vector2(237.99081420898438f, 219.65086f)),
                new OsuReplayFrame(17, new Vector2(238.7259063720703f, 216.71044921875f)), new OsuReplayFrame(16, new Vector2(238.72589111328125f, 214.50539f)),
                new OsuReplayFrame(19, new Vector2(236.52069091796875f, 213.03523f)), new OsuReplayFrame(17, new Vector2(233.58042907714844f, 212.30027770996094f)),
                new OsuReplayFrame(18, new Vector2(229.90505981445312f, 210.83003f)), new OsuReplayFrame(17, new Vector2(225.4946746826172f, 208.6248321533203f)),
                new OsuReplayFrame(17, new Vector2(221.81932067871094f, 207.15467834472656f)), new OsuReplayFrame(18, new Vector2(218.1439666748047f, 206.4195098876953f)),
                new OsuReplayFrame(19, new Vector2(212.99847412109375f, 204.94948f)), new OsuReplayFrame(19, new Vector2(210.05819702148438f, 204.94948f)),
                new OsuReplayFrame(19, new Vector2(207.85299682617188f, 204.94948f)), new OsuReplayFrame(18, new Vector2(204.91273498535156f, 209.35989379882812f)),
                new OsuReplayFrame(17, new Vector2(203.4426f, 213.03523f)), new OsuReplayFrame(18, new Vector2(202.70752f, 215.24043f)),
                new OsuReplayFrame(17, new Vector2(202.70752f, 215.97551f)), new OsuReplayFrame(19, new Vector2(204.17764f, 216.71059f)),
                new OsuReplayFrame(19, new Vector2(207.11792f, 217.44566f)), new OsuReplayFrame(17, new Vector2(210.0582f, 217.44566f)),
                new OsuReplayFrame(16, new Vector2(211.52835f, 217.44566f)), new OsuReplayFrame(19, new Vector2(212.26343f, 214.50539f)),
                new OsuReplayFrame(19, new Vector2(211.52835f, 211.56511f)), new OsuReplayFrame(18, new Vector2(204.91272f, 208.62483f)),
                new OsuReplayFrame(17, new Vector2(197.56204f, 206.41963f)), new OsuReplayFrame(19, new Vector2(193.15161f, 205.68456f)),
                new OsuReplayFrame(19, new Vector2(187.27106f, 207.88976f)), new OsuReplayFrame(19, new Vector2(180.65546f, 217.44566f)),
                new OsuReplayFrame(17, new Vector2(184.33078f, 229.94182f)), new OsuReplayFrame(16, new Vector2(184.33078f, 229.94182f)),
                new OsuReplayFrame(17, new Vector2(184.33078f, 229.94182f)), new OsuReplayFrame(17, new Vector2(191.68149f, 232.8821f)), new OsuReplayFrame(17, new Vector2(203.4426f, 235.0873f)),
                new OsuReplayFrame(17, new Vector2(214.46863f, 235.0873f)), new OsuReplayFrame(17, new Vector2(223.28946f, 229.94182f)),
                new OsuReplayFrame(17, new Vector2(229.17001f, 220.38594f)), new OsuReplayFrame(19, new Vector2(232.11026f, 209.35991f)),
                new OsuReplayFrame(18, new Vector2(226.22974f, 198.33386f)), new OsuReplayFrame(17, new Vector2(215.93875f, 191.71825f)),
                new OsuReplayFrame(19, new Vector2(213.73355f, 191.71825f)), new OsuReplayFrame(17, new Vector2(208.58807f, 191.71825f))
            ];
            string result = mod.DecodedString.Invoke(new List<ReplayFrame>(frames));
            Assert.AreEqual(result, "Test");
        }
    }
}
