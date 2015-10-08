using System.Collections.Generic;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Linear.Heuristics
{
    // The four options that are available when selecting a new vertex for the ordering, if no trivial case is selected

    public static class Heuristics
    {
        /*************************/
        // Candidate strategy
        /*************************/
        public static BitSet Candidates(Datastructures.Graph graph, BitSet left, BitSet right, CandidateStrategy candidateStrategy)
        {
            BitSet nl = graph.Neighborhood(left) * right;
            return candidateStrategy == CandidateStrategy.All ?
                right : (nl + graph.Neighborhood(nl)) * right;
        }

        /*************************/
        // Trivial cases
        /*************************/
        public static int TrivialCases(Datastructures.Graph graph, BitSet left, BitSet right)
        {
            int chosen = -1;

            // Check if any vertex in right belongs to one of the trivial cases, if yes then we can add this vertex directly to the sequence
            foreach (int v in right)
            {
                // Trivial case 1. If the neighbors of a vertex v in right are all contained in left, then select v
                // What this means is that v has an empty neighborhood, thus it will not add anything to the boolean-dimension
                if ((graph.OpenNeighborhood(v) - left).IsEmpty)
                {
                    chosen = v;

                    break;
                }

                bool stop = false;
                // 2. If there are two vertices, v in right and u in left, such that N(v) * right == (N(u)\v) * right, 
                // then v is selected as our next vertex
                // What this means is that all neighbors of v are already 'represented' by u, thus making the choice for v will not add anything to the dimension
                foreach (int u in left)
                {
                    BitSet nv = graph.OpenNeighborhood(v) * right;  // N(v) * right
                    BitSet nu = (graph.OpenNeighborhood(u) - v) * right;    // (N(u)\v) * right
                    if (nv.Equals(nu))
                    {
                        chosen = v;
                        stop = true;
                        break;
                    }
                }

                if (stop) break;
            }

            return chosen;
        }

        /*************************/
        // Selecting initial vertices
        /*************************/

        // Returns a vertex on the last level of a Breadth-first search (BFS)
        public static int Bfs(Datastructures.Graph graph, int init)
        {
            // BFS working queue
            Queue<int> queue = new Queue<int>();
            // Vertices that have already been visited
            Set<int> visited = new Set<int>();

            // Initial vertex is given as input
            visited.Add(init);
            queue.Enqueue(init);
            int current = init;

            // While there is still a vertex in the queue...
            while (queue.Count > 0)
            {
                //... select the first vertex and process it
                current = queue.Dequeue();
                foreach (int w in graph.OpenNeighborhood(current))
                {
                    // Enqueue all neighbors that have not been processed yet
                    if (!visited.Contains(w))
                    {
                        visited.Add(w);
                        queue.Enqueue(w);
                    }
                }
            }
            // This is the last vertex that has been processed, thus a vertex that is on the last level of the BFS search
            return current;        
        }
    }
}
