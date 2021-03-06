using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UglyToad.WiseOak
{
    public static class Trainer
    {
        public static DecisionTree Train(IReadOnlyList<IReadOnlyList<string>> rawData, int classIndex, Options? options = null)
        {
            if (rawData == null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }

            if (rawData.Count == 0)
            {
                return DecisionTree.Empty;
            }

            var enumLookup = new Dictionary<int, Dictionary<string, int>>();

            var recordLength = rawData[0].Count;

            if (classIndex >= recordLength || classIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(classIndex), $"The provided class index {classIndex} was not in the range of columns (0 <= x < {recordLength}).");
            }

            var data = new double[rawData.Count][];
            var classes = new int[rawData.Count];

            for (var rowIndex = 0; rowIndex < rawData.Count; rowIndex++)
            {
                var record = rawData[rowIndex];

                var rowData = new double[recordLength - 1];
                data[rowIndex] = rowData;

                if (record.Count != recordLength)
                {
                    throw new ArgumentException($"Row {rowIndex} had a different number of columns {record.Count} than expected {recordLength}.", nameof(rawData));
                }

                for (var colIndex = 0; colIndex < record.Count; colIndex++)
                {
                    var value = record[colIndex];

                    if (!double.TryParse(value, out var doubleValue))
                    {
                        if (!enumLookup.TryGetValue(colIndex, out var enumToValMap))
                        {
                            var comparer = options?.EnumsAreCaseSensitive == true ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
                            enumToValMap = new Dictionary<string, int>(comparer);
                            enumLookup[colIndex] = enumToValMap;
                        }

                        if (!enumToValMap.TryGetValue(value, out var enumValue))
                        {
                            if (options?.StringToValueTransformerColumnMap != null
                            && options.StringToValueTransformerColumnMap.TryGetValue(colIndex, out var transformer))
                            {
                                enumValue = transformer(value);
                            }
                            else
                            {
                                enumValue = enumToValMap.Count;
                            }

                            enumToValMap[value] = enumValue;
                        }

                        doubleValue = enumValue;
                    }

                    if (colIndex == classIndex)
                    {
                        classes[rowIndex] = (int)doubleValue;
                        continue;
                    }

                    if (colIndex > classIndex)
                    {
                        rowData[colIndex - 1] = doubleValue;
                    }
                    else
                    {
                        rowData[colIndex] = doubleValue;
                    }
                }
            }

            return Train(data, classes, options);
        }

        public static DecisionTree Train(double[][] data, int[] classes, Options? options = null)
        {
            if (options == null)
            {
                options = new Options();
            }

            var random = new Random(options.RandomSeed);

            var outputLog = options.OutputLogAction ?? (s => Trace.WriteLine(s));

            var maxDepth = options.MaximumDepthToCheck.HasValue && options.MaximumDepthToCheck.Value > 0 ?
                options.MaximumDepthToCheck.Value
                : data[0].Length;

            var bestDepth = 1;
            var bestAccuracy = new double?();

            var accuracies = new double[maxDepth];

            Parallel.For(
                1,
                maxDepth + 1,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = options.DegreeOfParallelism
                },
                depth =>
                {
                    outputLog($"Beginning training for depth: {depth}.");

                    var accuraciesLocal = new List<double>(options.NumberOfFolds);

                    foreach (var fold in CrossValidationFoldFactory.Get(data, classes, options.NumberOfFolds, random))
                    {
                        outputLog($"   D{depth} - Train fold {accuraciesLocal.Count + 1} of {options.NumberOfFolds}.");

                        var tree = DecisionTree.Build(
                            fold.Train,
                            fold.TrainClasses,
                            new DecisionTree.Options
                            {
                                MaxDepth = (uint)depth,
                                DegreeOfParalellism = options.DegreeOfParallelism
                            });

                        var wrong = 0;
                        for (var testRecordIndex = 0; testRecordIndex < fold.Test.Length; testRecordIndex++)
                        {
                            var record = fold.Test[testRecordIndex];
                            var expectedClass = fold.TestClasses[testRecordIndex];

                            var prediction = tree.Predict(record);

                            if (prediction != expectedClass)
                            {
                                wrong++;
                            }
                        }

                        var accuracyOnFold = (fold.TestClasses.Length - wrong) / (double)fold.TestClasses.Length;

                        outputLog($"      D{depth} - Accuracy was: {accuracyOnFold}.");

                        accuraciesLocal.Add(accuracyOnFold);
                    }

                    var thisAccuracy = accuraciesLocal.Average();
                    accuracies[depth - 1] = thisAccuracy;

                    outputLog($"   D{depth} - Overall accuracy for depth {depth} was: {thisAccuracy}.");

                    if (!bestAccuracy.HasValue || thisAccuracy > bestAccuracy.Value)
                    {
                        bestAccuracy = thisAccuracy;
                        bestDepth = depth;
                    }
                });

            outputLog($"Best depth was {bestDepth} with accuracy: {bestAccuracy.GetValueOrDefault()}.");

            var table = string.Join("\r\n", accuracies.Select((x, i) => $"{i + 1}\t{x}"));
            outputLog($"Depth\tAccuracy\r\n{table}");

            return DecisionTree.Build(data, classes, new DecisionTree.Options
            {
                MaxDepth = (uint)bestDepth,
                DegreeOfParalellism = options.DegreeOfParallelism
            });
        }

        public class Options
        {
            public bool EnumsAreCaseSensitive { get; set; }

            public Action<string>? OutputLogAction { get; set; }

            public Dictionary<int, Func<string, int>>? StringToValueTransformerColumnMap { get; set; }

            public int NumberOfFolds { get; set; } = 10;

            public int DegreeOfParallelism { get; set; } = 1;

            public int RandomSeed { get; set; } = 164562;

            public int? MaximumDepthToCheck { get; set; }
        }
    }
}
