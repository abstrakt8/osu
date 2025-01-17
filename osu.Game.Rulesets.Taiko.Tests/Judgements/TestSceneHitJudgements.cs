// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Judgements
{
    public class TestSceneHitJudgements : JudgementTest
    {
        [Test]
        public void TestHitCentreHit()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(hit_time, TaikoAction.LeftCentre),
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Centre,
                StartTime = hit_time
            }));

            AssertJudgementCount(1);
            AssertResult<Hit>(0, HitResult.Great);
        }

        [Test]
        public void TestHitRimHit()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(hit_time, TaikoAction.LeftRim),
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Rim,
                StartTime = hit_time
            }));

            AssertJudgementCount(1);
            AssertResult<Hit>(0, HitResult.Great);
        }

        [Test]
        public void TestMissHit()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0)
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Centre,
                StartTime = hit_time
            }));

            AssertJudgementCount(1);
            AssertResult<Hit>(0, HitResult.Miss);
        }

        [Test]
        public void TestHitStrongHitWithOneKey()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(hit_time, TaikoAction.LeftCentre),
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Centre,
                StartTime = hit_time,
                IsStrong = true
            }));

            AssertJudgementCount(2);
            AssertResult<Hit>(0, HitResult.Great);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);
        }

        [Test]
        public void TestHitStrongHitWithBothKeys()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(hit_time, TaikoAction.LeftCentre, TaikoAction.RightCentre),
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Centre,
                StartTime = hit_time,
                IsStrong = true
            }));

            AssertJudgementCount(2);
            AssertResult<Hit>(0, HitResult.Great);
            AssertResult<StrongNestedHitObject>(0, HitResult.LargeBonus);
        }

        [Test]
        public void TestMissStrongHit()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
            }, CreateBeatmap(new Hit
            {
                Type = HitType.Centre,
                StartTime = hit_time,
                IsStrong = true
            }));

            AssertJudgementCount(2);
            AssertResult<Hit>(0, HitResult.Miss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);
        }
    }
}
