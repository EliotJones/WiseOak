using Xunit;

namespace UglyToad.WiseOak.Tests.Unit
{
    public class GiniImpurityTests
    {
        [Theory]
        [InlineData(5, 0, 0)]
        [InlineData(0, 5, 0)]
        [InlineData(1, 5, 0.278)]
        [InlineData(109, 468, 0.3064)]
        [InlineData(233, 81, 0.3828)]
        public void Calculate2Class(int class1, int class2, double expected)
        {
            var result = GiniImpurity.CalculateGiniImpurity(class1, class2);

            Assert.Equal(expected, result, DoubleComparer.Instance);
        }
    }
}
