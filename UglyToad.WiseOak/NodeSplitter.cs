using System;
using System.Collections.Generic;

namespace UglyToad.WiseOak
{
    internal static class NodeSplitter
    {
        public static DecisionHolder? Split(
            Dictionary<int, int> classListIndices,
            int numberOfDimensions,
            bool[] isRecordActive,
            double[][] data,
            int[] classes)
        {
            var decision = default(DecisionHolder?);

            for (var dimensionIndex = 0; dimensionIndex < numberOfDimensions; dimensionIndex++)
            {
                var resultInDimension = SplitSingleDimension(data, classes, classListIndices, isRecordActive, dimensionIndex);

                if (resultInDimension.HasValue && (!decision.HasValue || resultInDimension.Value.Score > decision.Value.Score))
                {
                    decision = resultInDimension;
                }
            }

            return decision;
        }

        public static DecisionHolder? SplitSingleDimension(
            double[][] data,
            int[] classes,
            Dictionary<int, int> classListIndices,
            bool[] isRecordActive,
            int dimensionIndex)
        {
            if (data.Length != classes.Length)
            {
                throw new InvalidOperationException($"Data and class mismatch. Classes: {classes.Length}, Data: {data.Length}.");
            }

            var classCounts = new int[classListIndices.Count];
            for (var i = 0; i < classes.Length; i++)
            {
                var c = classes[i];
                if (isRecordActive[i])
                {
                    classCounts[classListIndices[c]]++;
                }
            }

            // Key is split at, value is Gini gain, higher gain is better split.
            var bestSplitScore = 0d;
            var bestSplitLocation = new double?();
            var bestLeft = new int?();
            var bestRight = new int?();

            var giniImpurityRaw = GiniImpurity.CalculateGiniImpurity(classCounts);

            if (giniImpurityRaw == 0)
            {
                // No improvement possible in this node for this dimension.
                return null;
            }

            var leftClassCounts = new int[classCounts.Length];
            var rightClassCounts = new int[classCounts.Length];

            var visited = new HashSet<double>();

            for (var i = 0; i < data.Length; i++)
            {
                if (!isRecordActive[i])
                {
                    continue;
                }

                var splitAt = data[i][dimensionIndex];

                if (visited.Contains(splitAt))
                {
                    continue;
                }

                visited.Add(splitAt);

                var leftTotal = 0;
                var rightTotal = 0;

                for (var j = 0; j < data.Length; j++)
                {
                    if (!isRecordActive[j])
                    {
                        continue;
                    }

                    var value = data[j][dimensionIndex];
                    var c = classes[j];

                    if (value <= splitAt)
                    {
                        leftClassCounts[classListIndices[c]]++;
                        leftTotal++;
                    }
                    else
                    {
                        rightClassCounts[classListIndices[c]]++;
                        rightTotal++;
                    }
                }

                var leftGini = GiniImpurity.CalculateGiniImpurity(leftClassCounts);
                var rightGini = GiniImpurity.CalculateGiniImpurity(rightClassCounts);

                var gain = giniImpurityRaw
                           - ((leftTotal / (double) data.Length) * leftGini)
                           - ((rightTotal / (double) data.Length) * rightGini);

                if (gain > bestSplitScore)
                {
                    bestSplitScore = gain;
                    bestSplitLocation = splitAt;
                    
                    var (left, right) = GetBestClassForSplit(leftClassCounts, rightClassCounts, classListIndices);
                    bestLeft = left;
                    bestRight = right;
                }

                if (i < data.Length - 1)
                {
                    for (int j = 0; j < leftClassCounts.Length; j++)
                    {
                        leftClassCounts[j] = 0;
                        rightClassCounts[j] = 0;
                    }
                }
            }

            if (!bestSplitLocation.HasValue)
            {
                return null;
            }

            return new DecisionHolder(bestSplitLocation.Value, bestSplitScore, dimensionIndex,  bestLeft!.Value, bestRight!.Value);
        }

        private static (int leftClass, int rightClass) GetBestClassForSplit(int[] leftClassCounts, int[] rightClassCounts, Dictionary<int, int> classToIndexMap)
        {
            var leftMax = int.MinValue;
            var leftMaxIndex = 0;
            for (int i = 0; i < leftClassCounts.Length; i++)
            {
                if (leftClassCounts[i] > leftMax)
                {
                    leftMax = leftClassCounts[i];
                    leftMaxIndex = i;
                }
            }

            var rightMax = int.MinValue;
            var rightMaxIndex = 0;
            for (int i = 0; i < rightClassCounts.Length; i++)
            {
                if (rightClassCounts[i] > rightMax)
                {
                    rightMax = rightClassCounts[i];
                    rightMaxIndex = i;
                }
            }

            var leftClass = 0;
            var rightClass = 0;
            foreach (var kvp in classToIndexMap)
            {
                if (kvp.Value == leftMaxIndex)
                {
                    leftClass = kvp.Key;
                }

                if (kvp.Value == rightMaxIndex)
                {
                    rightClass = kvp.Key;
                }
            }

            return (leftClass, rightClass);
        }
    }
}
