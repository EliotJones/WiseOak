using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UglyToad.WiseOak
{
    internal class CrossValidationFoldFactory
    {
        public static IEnumerable<Fold> Get(double[][] data, int[] classes, int numberOfFolds, Random random)
        {
            if (numberOfFolds <= 0)
            {
                yield break;
            }

            if (numberOfFolds == 1)
            {
                yield return new Fold(data, classes, new double[0][], new int[0]);
                yield break;
            }

            var sizeOfFold = data.Length / numberOfFolds;

            // Add any extra data to the test fold.
            var sizeOfTestFold = data.Length - ((numberOfFolds - 1) * sizeOfFold);

            var foldIndices = CreateEmptyFoldIndexArray(data);
            FillFoldIndexArray(foldIndices, numberOfFolds, random);

            var results = new Fold[numberOfFolds];

            for (var i = 0; i < numberOfFolds; i++)
            {
                var trainData = new double[sizeOfFold * (numberOfFolds - 1)][];
                var trainClasses = new int[trainData.Length];

                var testData = new double[sizeOfTestFold][];
                var testClasses = new int[testData.Length];

                var testIndex = 0;
                var trainIndex = 0;

                for (var j = 0; j < data.Length; j++)
                {
                    var fold = foldIndices[j];
                    if (fold == i || fold < 0)
                    {
                        var myIndex = testIndex;
                        testData[myIndex] = data[j];
                        testClasses[myIndex] = classes[j];
                        testIndex++;
                    }
                    else
                    {
                        var myIndex = trainIndex;
                        trainData[myIndex] = data[j];
                        trainClasses[myIndex] = classes[j];
                        trainIndex++;
                    }
                }

                results[i] = new Fold(trainData, trainClasses, testData, testClasses);
            }

            foreach (var result in results)
            {
                yield return result;
            }
        }

        private static int[] CreateEmptyFoldIndexArray(double[][] data)
        {
            var foldIndices = new int[data.Length];

            for (var i = 0; i < foldIndices.Length; i++)
            {
                foldIndices[i] = -1;
            }

            return foldIndices;
        }

        private static void FillFoldIndexArray(int[] foldIndices, int numberOfFolds, Random random)
        {
            var fisherYatesLength = foldIndices.Length;

            var sizeOfFold = foldIndices.Length / numberOfFolds;

            for (var foldIndex = 0; foldIndex < numberOfFolds; foldIndex++)
            {
                for (var foldItemIndex = 0; foldItemIndex < sizeOfFold; foldItemIndex++)
                {
                    var target = random.Next(fisherYatesLength);

                    // Find the non-assigned value;
                    var count = 0;
                    for (var k = 0; k < foldIndices.Length; k++)
                    {
                        if (foldIndices[k] >= 0)
                        {
                            continue;
                        }

                        if (count == target)
                        {
                            foldIndices[k] = foldIndex;
                            break;
                        }

                        count++;
                    }

                    fisherYatesLength--;
                }
            }
        }

        public class Fold
        {
            public double[][] Train { get; }

            public int[] TrainClasses { get; }

            public double[][] Test { get; }

            public int[] TestClasses { get; }

            public Fold(double[][] train, int[] trainClasses, double[][] test, int[] testClasses)
            {
                Train = train;
                TrainClasses = trainClasses;
                Test = test;
                TestClasses = testClasses;
            }
        }
    }
}