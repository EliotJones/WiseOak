namespace UglyToad.WiseOak
{
    internal readonly struct DecisionHolder
    {
        public readonly double SplitAt;

        public readonly double Score;

        public readonly int DimensionIndex;

        public readonly int LeftClass;

        public readonly int RightClass;

        public DecisionHolder(double splitAt, double score, int dimensionIndex, int leftClass, int rightClass)
        {
            SplitAt = splitAt;
            Score = score;
            DimensionIndex = dimensionIndex;
            LeftClass = leftClass;
            RightClass = rightClass;
        }
    }
}