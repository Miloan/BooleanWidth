/*************************/
// Frank van Houten, 2014 - 2015
//
// The Tomita algorithm is originally used to find all maximal cliques in a graph,
// here the algorithm is modified so that it can count the number of maximal independent sets.
// The algorithm is very similar to the Bron-Kerbosch algorithm, but uses pivoting to avoid expanding a lot of independent sets that will never be maximal.
/*************************/

using System;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.Maximal_IS_Counting
{
    public static class Tomita
    {
        // Initial call
        public static long Compute(Graph graph)
        {
            return Compute(graph, new BitSet(0, graph.Size), graph.Vertices, new BitSet(0, graph.Size));
        }

        // Counts the number of independent sets in a graph, such that:
        // - All vertices in R are included in the independent set, initially empty
        // - Some of the vertices in P are included in the independent set, initially all vertices of the graph
        // - None of the vertices in X are included in the independent set, initially empty
        private static long Compute(Graph graph, BitSet r, BitSet p, BitSet x)
        {
            // Base case, when P and X are both empty we cannot expand the IS
            if (p.IsEmpty && x.IsEmpty)
                return 1;

            long count = 0;
            int pivot = -1;
            int min = int.MaxValue;

            // Procedure to find a pivot
            // The idea is that in any maximal IS, either vertex u or a neighbor of u is included (else we could expand by adding u to the IS)
            // We find the u with the smallest neighborhood, so that we will keep the number of branches low
            foreach (int u in (p + x))
            {
                int size = (p * graph.OpenNeighborhood(u)).Count;
                if (size < min)
                {
                    min = size;
                    pivot = u;
                }
            }

            // There should always be a pivot after the selection procedure, else P and X should both have been empty
            if (pivot == -1)
                throw new Exception("Pivot has not been selected");

            // Foreach vertex v in the set containing the legal choices of the the closed neighborhood of the pivot,
            // we include each choice in the IS and compute how many maximal IS will include v by going into recursion
            foreach (int v in (p * graph.ClosedNeighborhood(pivot)))
            {
                count += Compute(graph, r + v, p - graph.ClosedNeighborhood(v), x - graph.OpenNeighborhood(v));
                p -=v;
                x += v;
            }

            return count;
        }
    }
}
