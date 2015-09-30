/*************************/
// Frank van Houten, 2014 - 2015
//
// The IUN Heuristic, implementation of algorithm 8 and 9 of the master thesis
/*************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;

namespace BooleanWidth.Algorithms.BooleanWidth.Linear.Heuristics
{
    public static class IunHeuristic
    {
        public static LinearDecomposition Compute(Datastructures.Graph graph, CandidateStrategy candidateStrategy, InitialVertexStrategy initialVertexStrategy)
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
                            //Parallel.ForEach(connectedComponent, vertex =>
                            //{
                            //    List<int> temp = ComputeSequence(graph, connectedComponent, candidateStrategy, vertex, out tempValue);
                            //    if (tempValue < minValue)
                            //    {
                            //        minValue = tempValue;
                            //        minList = temp;
                            //    }
                            //});
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
                    case InitialVertexStrategy.Bfs:
                        {
                            int init = Heuristics.Bfs(graph, connectedComponent.Last());
                            sequence.AddRange(ComputeSequence(graph, connectedComponent, candidateStrategy, init, out tempValue));
                        }
                        break;
                    case InitialVertexStrategy.DoubleBfs:
                        {
                            int init = Heuristics.Bfs(graph, Heuristics.Bfs(graph, connectedComponent.Last()));
                            sequence.AddRange(ComputeSequence(graph, connectedComponent, candidateStrategy, init, out tempValue));
                        }
                        break;
                }
            }
            return new LinearDecomposition(graph, sequence);
        }

        private static List<int> ComputeSequence(Datastructures.Graph graph, BitSet connectedComponent, CandidateStrategy candidateStrategy, int init, out int value)
        {
            int n = graph.Size;
            List<int> sequence = new List<int>() { init };
            BitSet left = new BitSet(0, n, new int[] { init });
            BitSet right = connectedComponent - init;

            // Initially we store the empty set and the set with init as the representative, ie N(init) * right
            Set<BitSet> unLeft = new Set<BitSet>() { new BitSet(0, n), graph.OpenNeighborhood(init) * right };
            value = int.MinValue;
            while (!right.IsEmpty)
            {
                Set<BitSet> unChosen = new Set<BitSet>();
                int chosen = Heuristics.TrivialCases(graph, left, right);

                if (chosen != -1)
                {
                    unChosen = IncrementUn(graph, left, unLeft, chosen);
                }
                // If chosen has not been set it means that no trivial case was found
                // Depending on the criteria for the next vertex we call a different algorithm
                else
                {
                    BitSet candidates = Heuristics.Candidates(graph, left, right, candidateStrategy);

                    int min = int.MaxValue;

                    foreach (int v in candidates)
                    {
                        Set<BitSet> unV = IncrementUn(graph, left, unLeft, v);
                        if (unV.Count < min)
                        {
                            chosen = v;
                            unChosen = unV;
                            min = unV.Count;
                        }
                    }
                }

                // This should never happen
                if (chosen == -1)
                    throw new Exception("No vertex is chosen for next step in the heuristic");

                // Add/remove the next vertex in the appropiate sets 
                sequence.Add(chosen);
                left += chosen;
                right -= chosen;
                unLeft = unChosen;
                value = Math.Max(unChosen.Count, value);
            }

            return sequence;
        }


        // The IUN selection method picks the next vertex based on a greedy choice, namely what is the vertex that will have the least
        // influence on the boolean dimension at this point, similar to the leastcutvalue.
        // In contrast to the leastcutvalue heuristic, we do not compute a bipartite graph, but rather construct and count the actual neighborhoods of possible cuts
        // Implementation of Algorithm 8 of the thesis
        private static Set<BitSet> IncrementUn(Datastructures.Graph graph, BitSet x, Set<BitSet> unX, int v)
        {
            Set<BitSet> u = new Set<BitSet>(new BitSet(0, graph.Size));

            foreach (BitSet s in unX)
            {
                u.Add(s - v);

                BitSet xComplement = (graph.Vertices - x);
                u.Add((s - v) + (graph.OpenNeighborhood(v) * (xComplement - v)));
            }

            return u;
        }
    }
}
