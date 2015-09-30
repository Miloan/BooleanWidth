using System;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    class Minimum : IOptimal
    {
        public Minimum() { }

        // The initial value is int.MaxValue, because we can only improve on this value
        public int PessimalValue
        {
            get { return int.MaxValue; }
        }

        public int Optimal(int x, int y)
        {
            return Math.Min(x, y);
        }
    }
}