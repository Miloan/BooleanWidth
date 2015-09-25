/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of Algorithm 10, 'GenerateVertexOrdering' by Sadia Sharmin in her PhD-thesis 'Practical Aspects of the Graph Parameter Boolean-Width'.
// Class for generating linear decompositions with a heuristic.
/*************************/

using System;
using System.Collections.Generic;


namespace Boolean_Width
{

    public static class SadiaHeuristics
    {
        public static LinearDecomposition Compute(Graph graph, ScoreFunction scoreFunction)
        {
            // We solve finding an ordering for each connected component of the graph seperately
            List<BitSet> connectedComponents = DepthFirstSearch.ConnectedComponents(graph);

            // The final sequence that we will return
            List<int> sequence = new List<int>();

            // Shorthand notation
            int n = graph.Size;

            foreach (BitSet connectedComponent in connectedComponents)
            {
                // Set of unprocessed vertices
                BitSet right = connectedComponent;

                // Set of processed vertices
                BitSet left = new BitSet(0, n);

                // The initial is selected by a certain strategy (in this case a double BFS)
                int init = Heuristics.BFS(graph, Heuristics.BFS(graph, connectedComponent.Last()));

                // Place init in the sequence
                sequence.Add(init);
                left += init;
                right -= init;

                // Continue while not all unprocessed vertices are moved
                while (!right.IsEmpty)
                {
                    int chosen = Heuristics.TrivialCases(graph, left, right);

                    // If chosen has not been set it means that no trivial case was found
                    // Depending on the criteria for the next vertex we call a different algorithm
                    if (chosen == -1)
                    {
                        switch (scoreFunction)
                        {
                            case ScoreFunction.LeastUncommonNeighbor:
                                chosen = LeastUncommonNeighbor(graph, left, right);
                                break;
                            case ScoreFunction.RelativeNeighborhood:
                                chosen = RelativeNeighborhood(graph, left, right);
                                break;
                            case ScoreFunction.LeastCutValue:
                                chosen = LeastCutValue(graph, left, right);
                                break;
                        }
                    }

                    // This should never happen; a selection criteria should always return some vertex
                    if (chosen == -1)
                        throw new Exception("No vertex is chosen for next step in the heuristic");

                    // Add/remove the next vertex in the appropiate sets 
                    sequence.Add(chosen);
                    left += chosen;
                    right -= chosen;

                }
            }
            return new LinearDecomposition(graph, sequence);
        }

        /*************************/
        // Selection algorithms for the next vertex in the sequence
        /*************************/

        // Implementation of Algorithm 11 of the PhD thesis by Sadia Sharmin
        // The LeastUncommonNeighbor selection method finds the vertex in right that has the fewest neighbors in right that are
        // not adjacent to a vertex in left. In other words, we add as few as possibly new vertices to the neighborhoods.
        private static int LeastUncommonNeighbor(Graph graph, BitSet left, BitSet right)
        {
            // Minimum symmetric difference found so far
            int minSymmDiff = int.MaxValue;

            // Vertex that we will choose
            int chosen = -1;

            // Set of vertices in right that are adjacent to vertices in left
            BitSet nl = graph.Neighborhood(left) * right;

            foreach (int v in right)
            {
                // Neighborhood of v in right
                BitSet nv = graph.OpenNeighborhood(v) * right;

                // Count the difference between N(v) and N(Left)
                int symmDiff = (nv - nl).Count;

                // Possibly update the best choice so far
                if (symmDiff < minSymmDiff)
                {
                    minSymmDiff = symmDiff;
                    chosen = v;
                }
            }

            return chosen;
        }

        // This is an improved version of Algorithm 12 of the PhD thesis by Sadia Sharmin, which runs in O(n) instead of O(n^2)
        // The RelativeNeighborhood selection method picks the next vertex based on the ratio of neighbors that are already contained in N(Left)
        // While in the PhD thesis the algorithm loops over all vertices w in N(v) * right to determine if they are internal/external, we can make this faster
        // by realizing that |((N(v) * right) \ (N(left) * right)| is already equal to the size of the external set. This is also equal to the symmetric difference.
        private static int RelativeNeighborhood(Graph graph, BitSet left, BitSet right)
        {
            // Minimum ratio found so far
            double minRatio = double.MaxValue;

            // Vertex that we will choose
            int chosen = -1;

            // Set of vertices in right that are adjacent to vertices in left
            BitSet nl = graph.Neighborhood(left) * right;

            Dictionary<int, double> ratios = new Dictionary<int, double>();
            foreach (int v in nl)
            {
                // Neighborhood of v
                BitSet nv = graph.OpenNeighborhood(v) * right;

                // Number of vertices outside of N(left) * right, but inside N(v) * right
                double external_v = (nv - nl).Count;

                // Number of vertices inside of both N(left) * right and N(v) * right
                double internal_v = nv.Count - external_v;

                if (internal_v == 0) internal_v = 1; // cant devide by zero!

                // Possibly update the minimum ratio found so far
                double ratio = external_v / internal_v;

                ratios[v+1] = ratio;


                if (ratio < minRatio)
                {
                    chosen = v;
                    minRatio = ratio;
                }
            }

            return chosen;
        }

        // Implementation of Algorithm 13 of the PhD thesis by Sadia Sharmin
        // The LeastCutValue selection method picks the next vertex based on a greedy choice, namely what is the vertex that will have the least
        // influence on the boolean dimension at this point.
        // While this generally gives great results, the runtime is very high because we construct multiple bipartite graphs during every iteration.
        private static int LeastCutValue(Graph graph, BitSet left, BitSet right)
        {
            // Minimum boolean dimension that we can have for our next cut
            long minBoolDim = long.MaxValue;

            // Vertex that we will choose
            int chosen = -1;

            // Foreach candidate vertex we construct a bipartite graph and count the number of minimal independent sets in this bipartite graph
             //This number is equal to the boolean-dimension at this cut
            foreach (int v in right)
            {
                // Construct the bipartite graph
                BipartiteGraph bg = new BipartiteGraph(graph, left + v, right - v);

                // Count the number of MIS in the bipartite graph
                long boolDim = CC_MIS.Compute(bg);

                // Possibly update the minimum value found so far
                if (boolDim < minBoolDim)
                {
                    chosen = v;
                    minBoolDim = boolDim;
                }
            }
          
            return chosen;
        }
    }
}
