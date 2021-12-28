// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    class ExpectedAttributes
    {
        // Bitmask isn't really future proof for mods such as "Blinds"
        public List<String> mods { get; set; }
        public double starRating { get; set; }

        // public double aimRating { get; set; }
        // public double speedRating { get; set; }
        // public double flashlightRating { get; set; }
    }

    class StarRatingTestCase
    {
        public string filename { get; set; }
        public List<ExpectedAttributes> cases { get; set; }
    }

    [TestFixture]
    public class TestCasesGenerator
    {
        StarRatingTestCase GenerateOne(string relativeFile)
        {
            var beatmap = getBeatmap(relativeFile);
            var calculator = new OsuDifficultyCalculator(new OsuRuleset(), beatmap);

            // There is actually calculator.CreateDifficultyAdjustmentModCombinations() but these are too much
            var modCombos = new[]
            {
                new Mod[] { },
                new Mod[] { new OsuModHardRock() },
                new Mod[] { new OsuModHalfTime() },
                new Mod[] { new OsuModDoubleTime() },
                new Mod[] { new OsuModFlashlight() },
                new Mod[] { new OsuModEasy() },
                new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
                new Mod[] { new OsuModHardRock(), new OsuModDoubleTime(), new OsuModFlashlight() },
            };

            var list = new List<ExpectedAttributes>();

            foreach (var modCombo in modCombos)
            {
                var modList = modCombo.ToList().ConvertAll(input => input.Acronym);
                var attributes = (OsuDifficultyAttributes)calculator.Calculate(modCombo);
                list.Add(new ExpectedAttributes()
                {
                    mods = modList,
                    starRating = attributes.StarRating,
                });
            }

            return new StarRatingTestCase()
            {
                cases = list,
                filename = relativeFile
            };
        }

        WorkingBeatmap getBeatmap(string file)
        {
            string absolutePath = Path.Combine(@"F:\My Drive\RewindTests\osu!\Songs", file);

            using (var resStream = new FileStream(absolutePath, FileMode.Open))
            using (var stream = new LineBufferedReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);

                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = false;

                return new TestWorkingBeatmap(decoder.Decode(stream))
                {
                    BeatmapInfo =
                    {
                        Ruleset = new OsuRuleset().RulesetInfo
                    }
                };
            }
        }

        // Should generate an ID (debugging purposes) along with the test case
        [TestCase]
        public void Test()
        {
            string[] files =
            {
                @"158023 UNDEAD CORPORATION - Everything will freeze/UNDEAD CORPORATION - Everything will freeze (Ekoro) [Time Freeze].osu",
                @"931596 Apol - Hidamari no Uta/Apol - Hidamari no Uta (-Keitaro) [Expert].osu",
                @"1010865 SHK - Violet Perfume [no video]/SHK - Violet Perfume (ktgster) [Insane].osu",
                @"863227 Brian The Sun - Lonely Go! (TV Size) [no video]/Brian The Sun - Lonely Go! (TV Size) (Nevo) [Fiery's Extreme].osu"
            };
            var testCases = new List<StarRatingTestCase>();

            foreach (var file in files)
            {
                testCases.Add(GenerateOne(file));
            }

            File.WriteAllText(
                @"E:\test.json",
                JsonConvert.SerializeObject(testCases)
            );
        }

        // TODO Single out a test case
    }
}
