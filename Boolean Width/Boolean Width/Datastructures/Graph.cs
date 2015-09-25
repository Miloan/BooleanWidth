/*************************/
// Frank van Houten, 2014 - 2015
//
// A graph is a collection of vertices together with an adjacencymatrix that keeps track of the neighbors of all vertices. 
/*************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace Boolean_Width
{
 
    public class Graph
    {
        /*************************/
        // Basic attributes
        /*************************/

        // All vertices contained in the graph
        public BitSet Vertices { get { return vertices.Copy();  } }
        private readonly BitSet vertices;

        // Adjacencymatrix for quick access to all neighbor of a certain vertex
        private BitSet[] AdjacencyMatrix;

        // Number of vertices in the graph
        public readonly int Size;

        /*************************/
        // Construction
        /*************************/

        // Constructor for a new graph of size n
        public Graph(int n)
        {
            vertices = new BitSet(0, n);
            AdjacencyMatrix = new BitSet[n];

            Size = n;

            for (int i = 0; i < n; i++)
            {
                vertices.Add(i);
                AdjacencyMatrix[i] = new BitSet(0, n);
            }
        }

        public Graph(Graph graph)
        {
            vertices = graph.vertices.Copy();

            AdjacencyMatrix = new BitSet[graph.Size];

            Size = graph.Size;

            for (int i = 0; i < Size; i++)
            {
                AdjacencyMatrix[i] = graph.AdjacencyMatrix[i].Copy();
            }
        }

        public void RemoveVertex (int i)
        {
            vertices.Remove(i);
            foreach (int j in AdjacencyMatrix[i])
            {
                Disconnect(i, j);
            }
        }

        // Removes an edge between vertex i and j by removing them from each others neighborhoods
        public void Disconnect(int i, int j)
        {
            AdjacencyMatrix[i].Remove(j);
            AdjacencyMatrix[j].Remove(i);
        }

        // Creates an edge between vertex i and j by adding them to each others neighborhoods
        public void Connect(int i, int j)
        {
            AdjacencyMatrix[i].Add(j);
            AdjacencyMatrix[j].Add(i);
        }

        /*************************/
        // Querying
        /*************************/

        // Returns the open neighborhood of a vertex i, ie. N(i)
        public BitSet OpenNeighborhood(int i)
        {
            return AdjacencyMatrix[i].Copy();
        }

        // Returns the closed neighborhood of vertex i, ie. N[i]
        public BitSet ClosedNeighborhood(int i)
        {
            return AdjacencyMatrix[i] + i;
        }

        // Returns the entire open neighborhood of a set of vertices by appending all neighborhoods to one set
        public BitSet Neighborhood(BitSet set)
        {
            BitSet result = new BitSet(0, Size);

            foreach (int v in set)
                result = result + OpenNeighborhood(v);

            return result;
        }

        // Returns the degree of a certain vertex with identifier i
        public int Degree(int i)
        {
            return AdjacencyMatrix[i].Count;
        }

        /*************************/
        // File IO
        /*************************/

        // Outputs a graph using the DGF format
        public void ToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine("p edges {0}", Size);
            foreach (int v in Vertices)
                foreach (int w in OpenNeighborhood(v))
                    if (v < w)  // avoids printing edges twice
                        sw.WriteLine("e {0} {1}", v + 1, w + 1);
            sw.Close();
        }

        public Graph Clone ()
        {
            return new Graph(this);
        }
    }
}

