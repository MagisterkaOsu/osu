// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mods
{
    public interface IDecodesReplay : IApplicableMod
    {
        Func<List<ReplayFrame>, string>? DecodedString { get; set; }
    }
}
