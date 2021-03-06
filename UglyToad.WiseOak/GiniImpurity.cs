namespace UglyToad.WiseOak
{
    public static class GiniImpurity
    {
        public static double CalculateGiniImpurity(int class1Count, int class2Count)
        {
            double total = class1Count + class2Count;

            if (total == 0)
            {
                return 0;
            }

            return ((class1Count / total) * (1 - (class1Count / total))) + ((class2Count / total) * (1 - (class2Count / total)));
        }

        public static double CalculateGiniImpurity(params int[] classCounts)
        {
            var sum = 0d;

            var totalCount = 0d;
            for (var i = 0; i < classCounts.Length; i++)
            {
                totalCount += classCounts[i];
            }

            if (totalCount == 0)
            {
                return 0;
            }

            for (var i = 0; i < classCounts.Length; i++)
            {
                var count = classCounts[i];

                var fractionalPart = count / totalCount;

                sum += fractionalPart * (1 - fractionalPart);
            }

            return sum;
        }
    }
}
