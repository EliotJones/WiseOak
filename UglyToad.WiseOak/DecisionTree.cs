using System;
using System.Collections.Generic;

namespace UglyToad.WiseOak
{
    public class DecisionTree
    {
        public DecisionTreeNode? Root { get; }

        public bool IsEmpty => Root == null;

        private DecisionTree(DecisionTreeNode? root)
        {
            Root = root;
        }

        public int Predict(double[] data)
        {
            if (IsEmpty)
            {
                return -1;
            }

            return Root!.Predict(data);
        }

        public static DecisionTree Build(double[][] data, bool[] classes, uint? maximumDepth = null)
        {
            var classesInt = new int[classes.Length];
            for (var i = 0; i < classes.Length; i++)
            {
                classesInt[i] = classes[i] ? 1 : 0;
            }

            return Build(data, classesInt, maximumDepth);
        }

        public static DecisionTree Build(double[][] data, int[] classes, uint? maximumDepth = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (classes == null)
            {
                throw new ArgumentNullException(nameof(classes));
            }

            if (classes.Length != data.Length)
            {
                throw new ArgumentException($"The number of classes {classes.Length} does not match the number of observations {data.Length}.");
            }

            if (maximumDepth == 0)
            {
                return new DecisionTree(null);
            }

            var isActive = new bool[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                isActive[i] = true;
            }

            var numberOfDimensions = data[0].Length;
            var classListIndexes = new Dictionary<int, int>();
            for (var i = 0; i < classes.Length; i++)
            {
                var c = classes[i];
                if (!classListIndexes.ContainsKey(c))
                {
                    var index = classListIndexes.Count;
                    classListIndexes[c] = index;
                }
            }

            var numberOfClasses = classListIndexes.Count;
            if (numberOfClasses <= 1)
            {
                return new DecisionTree(null);
            }

            var root = SplitRecursive(classListIndexes, numberOfDimensions, isActive, data, classes, maximumDepth, 0);

            return new DecisionTree(root);
        }

        private static DecisionTreeNode? SplitRecursive(Dictionary<int, int> classListIndices,
            int numberOfDimensions,
            bool[] isRecordActive,
            double[][] data,
            int[] classes,
            uint? maximumDepth,
            int currentDepth)
        {
            if (maximumDepth.HasValue && currentDepth >= maximumDepth)
            {
                return null;
            }

            var result = NodeSplitter.Split(classListIndices, numberOfDimensions, isRecordActive, data, classes);

            if (result == null)
            {
                return null;
            }

            if (result.Value.Score == 0)
            {
                return null;
            }

            var thisDecision = result.Value;

            for (var i = 0; i < isRecordActive.Length; i++)
            {
                var record = data[i];
                var value = record[thisDecision.DimensionIndex];
                isRecordActive[i] = value <= thisDecision.SplitAt;
            }

            var left = SplitRecursive(classListIndices, numberOfDimensions, isRecordActive, data, classes, maximumDepth, currentDepth + 1);

            for (var i = 0; i < isRecordActive.Length; i++)
            {
                var record = data[i];
                var value = record[thisDecision.DimensionIndex];
                isRecordActive[i] = value > thisDecision.SplitAt;
            }

            var right = SplitRecursive(classListIndices, numberOfDimensions, isRecordActive, data, classes, maximumDepth, currentDepth + 1);

            return new DecisionTreeNode(thisDecision.SplitAt, thisDecision.Score, thisDecision.DimensionIndex, thisDecision.LeftClass, thisDecision.RightClass, left, right);
        }
    }
}
