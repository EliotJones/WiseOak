using System;
using System.Collections.Generic;

namespace UglyToad.WiseOak
{
    public class DecisionTree
    {
        internal static readonly DecisionTree Empty = new DecisionTree(null);

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

        public static DecisionTree Build(double[][] data, bool[] classes, Options? options = null)
        {
            var classesInt = new int[classes.Length];
            for (var i = 0; i < classes.Length; i++)
            {
                classesInt[i] = classes[i] ? 1 : 0;
            }

            return Build(data, classesInt, options);
        }

        public static DecisionTree Build(double[][] data, int[] classes, Options? options = null)
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

            if (options?.MaxDepth == 0)
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

            var decisions = new List<(DecisionHolder, bool)>();

            var root = SplitRecursive(classListIndexes, numberOfDimensions, isActive, data, classes, options ?? new Options(), 0, decisions);

            return new DecisionTree(root);
        }

        private static DecisionTreeNode? SplitRecursive(Dictionary<int, int> classListIndices,
            int numberOfDimensions,
            bool[] isRecordActive,
            double[][] data,
            int[] classes,
            Options options,
            int currentDepth,
            List<(DecisionHolder decision, bool takeLeft)> previousDecisions)
        {
            if (currentDepth >= options.MaxDepth)
            {
                return null;
            }

            if (previousDecisions.Count > 0)
            {
                for (var i = 0; i < isRecordActive.Length; i++)
                {
                    var record = data[i];

                    for (var j = 0; j < previousDecisions.Count; j++)
                    {
                        var (decision, takeLeft) = previousDecisions[j];
                        var value = record[decision.DimensionIndex];
                        var isActive = takeLeft ? (value <= decision.SplitAt) : (value > decision.SplitAt);
                        if (!isActive)
                        {
                            isRecordActive[i] = false;
                            break;
                        }

                        isRecordActive[i] = true;
                    }
                }
            }

            var result = NodeSplitter.Split(classListIndices, numberOfDimensions, isRecordActive, data, classes, options.FeatureMask);

            if (result == null)
            {
                return null;
            }

            if (result.Value.Score == 0)
            {
                return null;
            }

            var thisDecision = result.Value;

            var nextLeft = new List<(DecisionHolder, bool takeLeft)>(previousDecisions)
            {
                (thisDecision, true)
            };

            var left = SplitRecursive(classListIndices, numberOfDimensions, isRecordActive, data, classes, options, currentDepth + 1, nextLeft);

            var nextRight = new List<(DecisionHolder, bool)>(previousDecisions)
            {
                (thisDecision, false)
            };

            var right = SplitRecursive(classListIndices, numberOfDimensions, isRecordActive, data, classes, options, currentDepth + 1, nextRight);

            return new DecisionTreeNode(thisDecision.SplitAt, thisDecision.Score, thisDecision.DimensionIndex, thisDecision.LeftClass, thisDecision.RightClass, left, right);
        }

        public class Options
        {
            public bool[]? FeatureMask { get; set; }

            public string[]? FeatureNames { get; set; }

            public uint? MaxDepth { get; set; }
        }
    }
}
