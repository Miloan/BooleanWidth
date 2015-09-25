/*************************/
// Frank van Houten, 2014 - 2015
//
// The IUN Heuristic, implementation of algorithm 8 and 9 of the master thesis
/*************************/

using System;
using System.Collections.Generic;
using System.IO;


namespace Boolean_Width
{
    public static class IUNHeuristic
    {
        public static LinearDecomposition Compute(Graph graph, CandidateStrategy candidateStrategy, InitialVertexStrategy initialVertexStrategy)
        {
            List<BitSet> connectedComponents = DepthFirstSearch.ConnectedComponents(graph);
            List<int> sequence = new List<int>();

            int tempValue;
            foreach (BitSet connectedComponent in connectedComponents)
            {
                switch (initialVertexStrategy)
                {
                    case InitialVertexStrategy.All:
                        {
                            List<int> minList = null;
                            int minValue = int.MaxValue;
                            foreach (int vertex in connectedComponent)
                            {
                                List<int> temp = ComputeSequence(graph, connectedComponent, candidateStrategy, vertex, out tempValue);
                                if (tempValue < minValue)
                                {
                                    minValue = tempValue;
                                    minList = temp;
                                }
                            }
                            sequence.AddRange(minList);
                        }
                        break;
                    case InitialVertexStrategy.BFS:
                        {
                            int init = Heuristics.BFS(graph, connectedComponent.Last());
                            sequence.AddRange(ComputeSequence(graph, connectedComponent, candidateStrategy, init, out tempValue));
                        }
                        break;
                    case InitialVertexStrategy.DoubleBFS:
                        {
                            int init = Heuristics.BFS(graph, Heuristics.BFS(graph, connectedComponent.Last()));
                            sequence.AddRange(ComputeSequence(graph, connectedComponent, candidateStrategy, init, out tempValue));
                        }
                        break;
                }
            }
            return new LinearDecomposition(graph, sequence);
        }

        private static List<int> ComputeSequence(Graph graph, BitSet connectedComponent, CandidateStrategy candidateStrategy, int init, out int value)
        {
            int n = graph.Size;
            List<int> sequence = new List<int>() { init };
            BitSet left = new BitSet(0, n) { init };
            BitSet right = connectedComponent - init;

            // Initially we store the empty set and the set with init as the representative, ie N(init) * right
            Set<BitSet> UN_left = new Set<BitSet>() { new BitSet(0, n), graph.OpenNeighborhood(init) * right };
            value = int.MinValue;
            while (!right.IsEmpty)
            {
                Set<BitSet> UN_chosen = new Set<BitSet>();
                int chosen = Heuristics.TrivialCases(graph, left, right);

                if (chosen != -1)
                {
                    UN_chosen = IncrementUN(graph, left, UN_left, chosen);
                }
                // If chosen has not been set it means that no trivial case was found
                // Depending on the criteria for the next vertex we call a different algorithm
                else
                {
                    BitSet candidates = Heuristics.Candidates(graph, left, right, candidateStrategy);

                    int min = int.MaxValue;

                    foreach (int v in candidates)
                    {
                        Set<BitSet> UN_v = IncrementUN(graph, left, UN_left, v);
                        if (UN_v.Count < min)
                        {
                            chosen = v;
                            UN_chosen = UN_v;
                            min = UN_v.Count;
                        }
                    }
                }

                // This should never happen
                if (chosen == -1)
                    throw new Exception("No vertex is chosen for next step in the heuristic");

                // Add/remove the next vertex in the appropiate sets 
                sequence.Add(chosen);
                left.Add(chosen);
                right.Remove(chosen);
                UN_left = UN_chosen;
                value = Math.Max(UN_chosen.Count, value);
            }

            return sequence;
        }


        // The IUN selection method picks the next vertex based on a greedy choice, namely what is the vertex that will have the least
        // influence on the boolean dimension at this point, similar to the leastcutvalue.
        // In contrast to the leastcutvalue heuristic, we do not compute a bipartite graph, but rather construct and count the actual neighborhoods of possible cuts
        // Implementation of Algorithm 8 of the thesis
        private static Set<BitSet> IncrementUN(Graph graph, BitSet X, Set<BitSet> UN_X, int v)
        {
            Set<BitSet> U = new Set<BitSet>(new BitSet(0, graph.Size));

            foreach (BitSet S in UN_X)
            {
                U.Add(S - v);

                BitSet _X = (graph.Vertices - X);
                U.Add((S - v) + (graph.OpenNeighborhood(v) * (_X - v)));
            }

            return U;
        }
    }
}
