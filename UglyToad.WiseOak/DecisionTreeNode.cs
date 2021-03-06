using System;

namespace UglyToad.WiseOak
{
    public class DecisionTreeNode
    {
        public double SplitAt { get; }

        public double Score { get; }

        public int DimensionIndex { get; }

        public int LeftClass { get; }

        public int RightClass { get; }

        public DecisionTreeNode? Left { get; }

        public DecisionTreeNode? Right { get; }

        public bool IsLeaf => Left == null && Right == null;

        public DecisionTreeNode(double splitAt, double score, int dimensionIndex, int leftClass, int rightClass)
        {
            if (dimensionIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimensionIndex), $"Dimension cannot be negative, got: {dimensionIndex}.");
            }

            SplitAt = splitAt;
            Score = score;
            DimensionIndex = dimensionIndex;
            LeftClass = leftClass;
            RightClass = rightClass;
        }

        public DecisionTreeNode(double splitAt, double score, int dimensionIndex,
            int leftClass,
            int rightClass,
            DecisionTreeNode? left,
            DecisionTreeNode? right)
            : this(splitAt, score, dimensionIndex, leftClass, rightClass)
        {
            Left = left;
            Right = right;
        }

        public int Predict(double[] data)
        {
            var value = data[DimensionIndex];
            if (value <= SplitAt)
            {
                if (Left != null)
                {
                    return Left.Predict(data);
                }

                return LeftClass;
            }

            if (Right != null)
            {
                return Right.Predict(data);
            }

            return RightClass;
        }
    }
}