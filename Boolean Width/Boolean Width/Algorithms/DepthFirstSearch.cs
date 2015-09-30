/*************************/
// Frank van Houten, 2014 - 2015
//
// Class for performing Depth-first searches on graph, which can be used to determine if a sub(graph) is connected 
// and it can retreive different connected components of a graph.
/*************************/

using System.Collections.Generic;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms
{
    public static class DepthFirstSearch
    {
        public static bool Connected(Datastructures.Graph graph)
        {
            return Connected(graph, graph.Vertices);
        }

        // Uses depth-first search to check if the graph induced by the subgraph given as a parameter is connected
        // In other words, we retreive all edges from the original graph and check the subgraph for connectedness
        public static bool Connected(Datastructures.Graph graph, BitSet subgraph)
        {
            // Vertices that are visited
            Set<int> visited = new Set<int>();

            // Stack of vertices yet to visit
            Stack<int> stack = new Stack<int>();

            // Initial vertex
            int s = subgraph.First();
            stack.Push(s);

            // Continue while there are vertices on the stack
            while (stack.Count > 0)
            {
                int v = stack.Pop();

                // If we have not encountered this vertex before, then we check for all neighbors if they are part of the subgraph
                // If a neighbor is part of the subgraph it means that we have to push it on the stack to explore it at a later stage
                if (!visited.Contains(v))
                {
                    visited.Add(v);

                    foreach (int w in graph.OpenNeighborhood(v))
                        if (subgraph.Contains(w))
                            stack.Push(w);
                }
            }

            // If we visited an equal number of vertices as there are vertices in the subgraph then the subgraph is connected
            return visited.Count == subgraph.Count;
        }

        public static List<BitSet> ConnectedComponents(Datastructures.Graph graph)
        {
            return ConnectedComponents(graph, graph.Vertices);
        }

        // Returns all connected components in a certain subgraph, where we define a subgraph by the vertices that are contained in it
        // We apply multiple dfs searches to find all connected parts of the graph
        public static List<BitSet> ConnectedComponents(Datastructures.Graph graph, BitSet subgraph)
        {
            // Each connected component is defined as a bitset, thus the result is a list of these bitsets
            List<BitSet> result = new List<BitSet>();

            // Working stack for the dfs algorithm
            Stack<int> stack = new Stack<int>();

            // Each vertex of the subgraph will either be explored, or it will be the starting point of a new dfs search
            BitSet todo = subgraph;

            while (!todo.IsEmpty)
            {
                int s = todo.First();
                stack.Push(s);

                // Start tracking the new component
                BitSet component = new BitSet(0, graph.Size);

                // Default dfs exploring part of the graph
                while (stack.Count > 0)
                {
                    int v = stack.Pop();

                    // If we have not encountered this vertex before (meaning it isn't in this component), then we check for all neighbors if they are part of the subgraph
                    // If a neighbor is part of the subgraph it means that we have to push it on the stack to explore it at a later stage for this component
                    if (!component.Contains(v))
                    {
                        component += v;

                        // Remove this vertex from the 'todo' list, since it can never be the starting point of a new component
                        todo -= v;

                        foreach (int w in graph.OpenNeighborhood(v))
                            if (subgraph.Contains(w))
                                stack.Push(w);
                    }
                }
                // The whole connected component has been found, so we can add it to the list of results
                result.Add(component);
            }
            return result;
        }
    }
}
