using System;
using System.Collections.Generic;

namespace UglyToad.WiseOak.Tests.Unit
{
    internal class DoubleComparer : IEqualityComparer<double>
    {
        public static readonly IEqualityComparer<double> Instance = new DoubleComparer();

        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < 0.01;
        }

        public int GetHashCode(double obj) => obj.GetHashCode();
    }
}