using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvSwan;
using Xunit;

namespace UglyToad.WiseOak.Tests.Unit
{
    public class DecisionTreeTests
    {
        [Fact]
        public void Build4dDataSet()
        {
            var data = new[]
            {
                new double []{ 1, 2, 2, 1 },
                new double []{ 1, 2, 3, 2 },
                new double []{ 1, 2, 2, 3 },
                new double []{ 2, 2, 2, 1 },
                new double []{ 2, 3, 2, 2 },

                new double []{ 1, 3, 2, 1 },
                new double []{ 1, 2, 3, 1 },
                new double []{ 2, 3, 1, 2 },
                new double []{ 1, 2, 2, 2 },
                new double []{ 1, 1, 3, 2 },

                new double []{ 2, 1, 2, 2 },
                new double []{ 1, 1, 2, 3 }
            };

            var classes = new[]
            {
                1, 1, 1, 1, 2,
                1, 2, 1, 1, 1,
                2, 1
            };

            var tree = DecisionTree.Build(data, classes);

            Assert.NotNull(tree);
            Assert.NotNull(tree.Root);
        }

        [Fact]
        public void Build2dDataSet()
        {
            // Built by estimating from the sample set in https://victorzhou.com/blog/gini-impurity/#picking-the-best-split
            var data = new[]
            {
                // Class 1
                new []{ 0.2, 1.5 },
                new []{ 0.5, 0.2 },
                new []{ 0.6, 1.2 },
                new []{ 1.0, 2.3 },
                new []{ 1.8, 0.3 },
                
                // Class 2
                new []{ 2.3, 1.6 },
                new []{ 2.4, 1.4 },
                new []{ 2.5, 3.1 },
                new []{ 2.5, 0.3 },
                new []{ 2.9, 2.1 }
            };

            var classes = new[]
            {
                1, 1, 1, 1, 1,
                2, 2, 2, 2, 2
            };

            var tree = DecisionTree.Build(data, classes);

            Assert.NotNull(tree);
            Assert.NotNull(tree.Root);
            Assert.False(tree.IsEmpty);

            Assert.True(tree.Root.IsLeaf);
            Assert.Null(tree.Root.Left);
            Assert.Null(tree.Root.Right);

            var predictionA = tree.Predict(new[] { 0.7, 5 });
            var predictionB = tree.Predict(new[] { 6, 0.3 });

            Assert.Equal(1, predictionA);
            Assert.Equal(2, predictionB);
        }

        [Fact]
        public void IngestData()
        {
            const string path = @"C:\git\csharp\UglyToad.WiseOak\datasets\bank-additional-full.csv";

            if (!File.Exists(path))
            {
                return;
            }

            using (var csv = Csv.Open(path, separator: ';', hasHeaderRow: true))
            {
                var rvs = csv.GetAllRowValues();

                var tree = Trainer.Train(rvs, rvs[0].Count - 1, new Trainer.Options
                {
                    StringToValueTransformerColumnMap = new Dictionary<int, Func<string, int>>
                    {
                        // Married
                        {2, x =>
                        {
                            switch (x)
                            {
                                case "married":
                                    return 1;
                                default:
                                    return 0;
                            }
                        }},
                        // Housing
                        {6, x => x == "yes" ? 1 : 0},
                        // Zero out the duration feature since it is not known beforehand and is effectively a "cheat" feature since it correlates highly with class.
                        {10, x => 0}
                    },
                    DegreeOfParallelism = 4,
                    MaximumDepthToCheck = 8
                });
                Assert.NotNull(tree);
                Assert.NotNull(tree.Root);
            }
        }

        [Fact]
        public async Task IrisTestData()
        {
            var path = Path.Combine(Path.GetTempPath(), "iris-wise-oak.csv");

            if (!File.Exists(path))
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://raw.githubusercontent.com/scikit-learn/scikit-learn/main/sklearn/datasets/data/iris.csv");

                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    await File.WriteAllTextAsync(path, await response.Content.ReadAsStringAsync());
                }
            }

            using (var csv = Csv.Open(path, ',', true))
            {
                var allRows = csv.GetAllRowValues();

                var tree = Trainer.Train(allRows, allRows[0].Count - 1, new Trainer.Options());

                Assert.NotNull(tree.Root);

                var wrong = 0;
                var data = new double[allRows[0].Count - 1];
                for (var i = 0; i < allRows.Count; i++)
                {
                    var row = allRows[i];
                    for (var j = 0; j < row.Count - 1; j++)
                    {
                        data[j] = double.Parse(row[j]);
                    }

                    var expectedClass = int.Parse(row[^1]);

                    var predicted = tree.Predict(data);

                    if (predicted != expectedClass)
                    {
                        wrong++;
                    }
                }

                var accuracy = 100 - ((wrong / (double) allRows.Count) * 100);

                Assert.True(accuracy > 90, $"Accuracy on Iris dataset fell below 90%: {accuracy}%");
            }
        }
    }
}