using System;
using System.Linq;
using Xunit;

namespace UglyToad.WiseOak.Tests.Unit
{
    public class CrossValidationFoldFactoryTests
    {
        [Fact]
        public void CanSplitUnevenDataSet()
        {
            var data = new[]
            {
                new double[] {1, 2, 3, 5},
                new double[] {3, 2, 2, 5},
                new double[] {1, 1, 1, 1},

                new double[] {1, 5, 0, 2},
                new double[] {4, 4, 3, 0},
                new double[] {6, 2, 2, 1}
            };

            var classes = new [] {1, 1, 1, 1, 0, 0};

            var random = new Random(25065);

            var folds = CrossValidationFoldFactory.Get(data, classes, 5, random).ToList();

            Assert.Equal(5, folds.Count);

            var first = folds[0];

            Assert.Equal(4, first.TrainClasses.Length);
            Assert.Equal(2, first.TestClasses.Length);
        }
    }
}
