// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Tests
{
    class ExpectedAttributes
    {
        // Bitmask isn't really future proof for mods such as "Blinds"
        public List<String>? mods { get; set; }
        public double starRating { get; set; }

        public double aimRating { get; set; }
        public double speedRating { get; set; }
        public double flashlightRating { get; set; }
    }

    class StarRatingTestCase
    {
        public string? filename { get; set; }
        public List<ExpectedAttributes>? cases { get; set; }
    }

    [TestFixture]
    public class TestCasesGenerator
    {
        private IResourceStore<byte[]> resourcesStore => new DllResourceStore(Assembly.GetAssembly(typeof(TestCasesGenerator)));

        StarRatingTestCase GenerateOne(string relativeFile)
        {
            var beatmap = getBeatmap(relativeFile);
            var calculator = new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);

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
                    aimRating = attributes.AimDifficulty,
                    speedRating = attributes.SpeedDifficulty,
                    flashlightRating = attributes.FlashlightDifficulty
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
            // string absolutePath = Path.Combine(@"Resources/Songs", file);
            string path = $"Resources/osu-testdata/Songs/{file}";
            Console.WriteLine(path);

            using (var resStream = resourcesStore.GetStream(path))
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
            var assembly = Assembly.GetAssembly(typeof(TestCasesGenerator))!;
            // TODO: Improve this
            string osuTestData = Path.Join(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assembly.Location)))), "Resources/osu-testdata");

            var testCases = new List<StarRatingTestCase>();
            var songsFolder = Path.Join(osuTestData, "Songs");

            foreach (var file in Directory.GetFiles(songsFolder))
            {
                testCases.Add(GenerateOne(Path.GetFileName(file)));
            }

            const string file_name = "20220928.json";
            string location = Path.Join(osuTestData, $"out/sr/{file_name}");

            using (var streamWriter = new StreamWriter(location))
            {
                streamWriter.Write(JsonConvert.SerializeObject(testCases));
            }
        }

        // TODO Single out a test case
    }
}
