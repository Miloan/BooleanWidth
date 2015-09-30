/*************************/
// Frank van Houten, 2014 - 2015
//
// Simple interface for solving an maximal or minimal vertex subsset problems
/*************************/

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public interface IOptimal
    {
        // Worst value that can be found in a table, used to indicate no solution can be found
        int PessimalValue { get; }

        // Determines the optimal value of two input integer x and y
        int Optimal(int x, int y);
    }

    // Implementation of the Optimal interface used for solving Minimum vertex subset problems

    // Implementation of the Optimal interface used for solving Maximum vertex subset problems
}
