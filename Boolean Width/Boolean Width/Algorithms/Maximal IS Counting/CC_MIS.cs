﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// The CC_MIS algorithm counts the number of independent sets in a recursive fashion.
// This algorithm does not actually return the independent sets; it only counts them.
/*************************/

using System;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.Maximal_IS_Counting
{
    public static class CcMis
    {
        // Initial call
        public static long Compute(Graph graph)
        {
            return Compute(graph, graph.Vertices, new BitSet(0, graph.Size), true);
        }

        // Counts the number of independent sets in a graph, such that:
        // - The vertices in P are legal choices for our IS, initially set to all vertices in the graph
        // - None of the vertices in X are used, initially empty
        // The performDFS boolean is used to check if we should perform a check for connectedness on this level of the recursion
        private static long Compute(Graph graph, BitSet p, BitSet x, bool performDfs)
        {
            // Base case, when P and X are both empty we cannot expand the IS
            if (p.IsEmpty && x.IsEmpty)
                return 1;

            // Base case, if a vertex w in X has no neighbor in P, then it means that this IS will never get maximal 
            // since we could always include w. Thus, the IS will not be valid and we return 0.
            foreach (int w in x)
                if ((graph.OpenNeighborhood(w) * p).IsEmpty)
                    return 0;

            long count = 0;

            // If a DFS is needed we check if the graph induced by (P + X) is still connected.
            // If the graph is disconnected, in components c1,...,cn then we can simply count the IS of all these components
            // after which we simply multiply these numbers.
            if (performDfs)
            {
                if (!DepthFirstSearch.Connected(graph, p + x))
                {
                    count = 1;

                    foreach (BitSet component in DepthFirstSearch.ConnectedComponents(graph, p + x))
                        count *= Compute(graph, component * p, component * x, false);

                    return count;
                }
            }

            // Select a pivot in P to branch on
            // In this case we pick the vertex with the largest degree
            int maxDegree = -1; ;
            int pivot = -1;
            foreach (int u in p)
            {
                int deg = graph.Degree(u);
                if (deg > maxDegree)
                {
                    maxDegree = deg;
                    pivot = u;
                }
            }

            // There should always be a pivot after the selection procedure
            if (pivot == -1)
                throw new Exception("Pivot has not been selected");

            // We branch on the pivot, one branch we include the pivot in the IS.
            // This might possibly disconnect the subgraph G(P + X), thus we set the performDFS boolean to true.
            count = Compute(graph, p - graph.ClosedNeighborhood(pivot), x - graph.OpenNeighborhood(pivot), true);

            // One branch we exclude the pivot of the IS. This will not cause the graph to get possibly disconnected
            count += Compute(graph, p - pivot, x + pivot, false);

            return count;
        }
    }
}
