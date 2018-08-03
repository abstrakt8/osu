﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class DrumRollTick : TaikoHitObject
    {
        /// <summary>
        /// Whether this is the first (initial) tick of the slider.
        /// </summary>
        public bool FirstTick;

        /// <summary>
        /// The length (in milliseconds) between this tick and the next.
        /// <para>Half of this value is the hit window of the tick.</para>
        /// </summary>
        public double TickSpacing;

        /// <summary>
        /// The time allowed to hit this tick.
        /// </summary>
        public double HitWindow => TickSpacing / 2;

        protected override Judgement CreateJudgement() => new TaikoDrumRollTickJudgement();
    }
}
