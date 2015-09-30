using System;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    class Maximum : IOptimal
    {
        public Maximum() { }

        // The initial value is int.MinValue, because we can only improve on this value
        public int PessimalValue
        {
            get { return int.MinValue; }
        }

        public int Optimal(int x, int y)
        {
            return Math.Max(x, y);
        }
    }
}