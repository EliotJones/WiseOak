using System.Collections.Generic;
using Xunit;

namespace UglyToad.WiseOak.Tests.Unit
{
    public class NodeSplitterTests
    {
        [Fact]
        public void SplitSingleNode1Dimension()
        {
            var data = new []
            {
                // Class 1
                new []{ 0.2 },
                new []{ 0.5 },
                new []{ 0.6 },
                new []{ 1.0 },
                new []{ 1.8 },
                
                // Class 2
                new []{ 2.3 }, 
                new []{ 2.4 },
                new []{ 2.5 },
                new []{ 2.5 },
                new []{ 2.9 }
            };

            var classes = new[]
            {
                0, 0, 0, 0, 0,
                1, 1, 1, 1, 1
            };

            var classToListIndex = new Dictionary<int, int>
            {
                {0, 0},
                {1, 1}
            };

            var isRecordActive = new[]
            {
                true, true, true, true, true,
                true, true, true, true, true
            };

            var bestSplit = NodeSplitter.Split(
                classToListIndex,
                1,
                isRecordActive,
                data,
                classes,
                null,
                1);

            Assert.NotNull(bestSplit);
            Assert.Equal(1.8, bestSplit.Value.SplitAt);
            Assert.Equal(0, bestSplit.Value.DimensionIndex);
            Assert.Equal(0, bestSplit.Value.LeftClass);
            Assert.Equal(1, bestSplit.Value.RightClass);
        }
    }
}