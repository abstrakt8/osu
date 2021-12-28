// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

namespace osu.Game.Rulesets.Osu.Tests
{
    internal class SRCase
    {
        // Bitmask isn't really future proof for mods such as "Blinds"
        public List<string> mods { get; set; }
        public double starRating { get; set; }

        public double aimRating { get; set; }
        public double speedRating { get; set; }
        public double flashlightRating { get; set; }
    }

    internal class SRTestSuite
    {
        public string filename { get; set; }
        public List<SRCase> cases { get; set; }
    }

    internal class PPCase
    {
        public List<string> mods { get; set; }
        public int combo { get; set; }
        public int countGreat { get; set; }
        public int countOk { get; set; }
        public int countMeh { get; set; }
        public int countMiss { get; set; }
        public double totalPP { get; set; }
    }

    internal class PPTestSuite
    {
        public string filename { get; set; }
        public List<PPCase> cases { get; set; }
    }

    [TestFixture]
    public class TestCasesGenerator
    {
        private IResourceStore<byte[]> resourcesStore => new DllResourceStore(Assembly.GetAssembly(typeof(TestCasesGenerator)));
        private Assembly assembly => Assembly.GetAssembly(typeof(TestCasesGenerator))!;

        // TODO: Improve this
        private string osuTestData => Path.Join(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assembly.Location)))), "Resources/osu-testdata");

        // There is actually calculator.CreateDifficultyAdjustmentModCombinations() but these are too much
        private readonly Mod[][] modCombos =
        {
            new Mod[] { },
            new[] { new OsuModHardRock() },
            new[] { new OsuModHalfTime() },
            new[] { new OsuModDoubleTime() },
            new[] { new OsuModFlashlight() },
            new[] { new OsuModFlashlight(), new OsuModHidden() },
            new[] { new OsuModEasy() },
            new[] { new OsuModHardRock(), new OsuModDoubleTime() },
            new[] { new OsuModHardRock(), new OsuModDoubleTime(), new OsuModFlashlight() },
            new[] { new OsuModHardRock(), new OsuModDoubleTime(), new OsuModFlashlight(), new OsuModHidden() },
            new[] { new OsuModRelax() }
        };

        [TestCase]
        public void GenerateSR()
        {
            var testCases = new List<SRTestSuite>();
            var songsFolder = Path.Join(osuTestData, "Songs");

            foreach (var file in Directory.GetFiles(songsFolder)) testCases.Add(GenerateSROne(Path.GetFileName(file)));

            const string file_name = "20220928.json";
            string location = Path.Join(osuTestData, $"out/sr/{file_name}");

            using (var streamWriter = new StreamWriter(location)) streamWriter.Write(JsonConvert.SerializeObject(testCases));
        }

        [TestCase]
        public void TestOneSR()
        {
            // Use it for debugging
            const string single = "SHK - Violet Perfume (ktgster) [Insane].osu";
            GenerateSROne(single);
        }

        [TestCase]
        public void GeneratePP()
        {
            var testSuites = new List<PPTestSuite>();
            string songsFolder = Path.Join(osuTestData, "Songs");

            foreach (string file in Directory.GetFiles(songsFolder)) testSuites.Add(GeneratePPOne(Path.GetFileName(file)));

            const string file_name = "20220928.json";
            string location = Path.Join(osuTestData, $"out/pp/{file_name}");

            using (var streamWriter = new StreamWriter(location)) streamWriter.Write(JsonConvert.SerializeObject(testSuites));
        }

        private SRTestSuite GenerateSROne(string relativeFile)
        {
            var beatmap = getBeatmap(relativeFile);
            var calculator = new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);
            var list = new List<SRCase>();

            foreach (var modCombo in modCombos)
            {
                var modList = modCombo.ToList().ConvertAll(input => input.Acronym);
                var attributes = (OsuDifficultyAttributes)calculator.Calculate(modCombo);
                list.Add(new SRCase
                {
                    mods = modList,
                    starRating = attributes.StarRating,
                    aimRating = attributes.AimDifficulty,
                    speedRating = attributes.SpeedDifficulty,
                    flashlightRating = attributes.FlashlightDifficulty
                });
            }

            return new SRTestSuite
            {
                cases = list,
                filename = relativeFile
            };
        }

        WorkingBeatmap getBeatmap(string file)
        {
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

        private PPTestSuite GeneratePPOne(string fileName)
        {
            var beatmap = getBeatmap(fileName);
            var difficultyCalculator = new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);
            var performanceCalculator = new OsuPerformanceCalculator();
            var list = new List<PPCase>();

            foreach (var modCombo in modCombos)
            {
                var acronyms = modCombo.ToList().ConvertAll(input => input.Acronym);
                var diffAttributes = (OsuDifficultyAttributes)difficultyCalculator.Calculate(modCombo);

                int fc = diffAttributes.MaxCombo;
                int n = diffAttributes.SliderCount + diffAttributes.HitCircleCount + diffAttributes.SpinnerCount;

                void addOne(int combo, int countGreat, int countOk, int countMeh, int countMiss)
                {
                    Debug.Assert(combo >= 0 && combo <= fc);
                    Debug.Assert(countGreat + countOk + countMeh + countMiss == n);
                    var scoreInfo = new ScoreInfo
                    {
                        Statistics = new Dictionary<HitResult, int>
                        {
                            [HitResult.Great] = countGreat,
                            [HitResult.Ok] = countOk,
                            [HitResult.Meh] = countMeh,
                            [HitResult.Miss] = countMiss
                        },
                        Combo = combo
                    };
                    var attributes = performanceCalculator.Calculate(scoreInfo, diffAttributes);
                    list.Add(new PPCase
                    {
                        combo = combo,
                        mods = acronyms,
                        countGreat = countGreat,
                        countOk = countOk,
                        countMeh = countMeh,
                        countMiss = countMiss,
                        totalPP = attributes.Total
                    });
                }

                addOne(fc, n, 0, 0, 0);
                if (n >= 2)
                    addOne(fc / 2, n - 2, 0, 0, 2);
                if (n >= 10)
                    addOne((int)Math.Floor(fc * 0.8), n - 11, 10, 0, 1);
                if (n >= 30)
                    addOne(fc - 1, n - 30, 0, 30, 0);
            }

            return new PPTestSuite
            {
                filename = fileName,
                cases = list
            };
        }
    }
}
