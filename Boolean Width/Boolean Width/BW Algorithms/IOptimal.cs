/*************************/
// Frank van Houten, 2014 - 2015
//
// Simple interface for solving an maximal or minimal vertex subsset problems
/*************************/
using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public interface IOptimal
    {
        // Worst value that can be found in a table, used to indicate no solution can be found
        int PessimalValue { get; }

        // Determines the optimal value of two input integer x and y
        int Optimal(int x, int y);
    }

    // Implementation of the Optimal interface used for solving Minimum vertex subset problems
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

    // Implementation of the Optimal interface used for solving Maximum vertex subset problems
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
