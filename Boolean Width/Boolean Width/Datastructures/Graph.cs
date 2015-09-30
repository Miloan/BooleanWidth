/*************************/
// Frank van Houten, 2014 - 2015
//
// A graph is a collection of vertices together with an adjacencymatrix that keeps track of the neighbors of all vertices. 
/*************************/

using System.Collections.Generic;
using System.IO;

namespace BooleanWidth.Datastructures
{
 
    public class Graph
    {
        /*************************/
        // Basic attributes
        /*************************/

        // All vertices contained in the graph
        public BitSet Vertices { get; private set; }

        // Adjacencymatrix for quick access to all neighbor of a certain vertex
        private BitSet[] _adjacencyMatrix;

        // Number of vertices in the graph
        public readonly int Size;

        /*************************/
        // Construction
        /*************************/

        // Constructor for a new graph of size n
        public Graph(int n)
        {
            _adjacencyMatrix = new BitSet[n];

            Size = n;

            List<int> verticesList = new List<int>();
            for (int i = 0; i < n; i++)
            {
                verticesList.Add(i);
                _adjacencyMatrix[i] = new BitSet(0, n);
            }
            Vertices = new BitSet(0, n, verticesList);
        }

        public Graph(Graph graph)
        {
            Vertices = graph.Vertices;

            _adjacencyMatrix = new BitSet[graph.Size];

            Size = graph.Size;

            for (int i = 0; i < Size; i++)
            {
                _adjacencyMatrix[i] = graph._adjacencyMatrix[i];
            }
        }

        public void RemoveVertex (int i)
        {
            Vertices -= i;
            foreach (int j in _adjacencyMatrix[i])
            {
                Disconnect(i, j);
            }
        }

        // Removes an edge between vertex i and j by removing them from each others neighborhoods
        public void Disconnect(int i, int j)
        {
            _adjacencyMatrix[i] -= j;
            _adjacencyMatrix[j] -= i;
        }

        // Creates an edge between vertex i and j by adding them to each others neighborhoods
        public void Connect(int i, int j)
        {
            _adjacencyMatrix[i] += j;
            _adjacencyMatrix[j] += i;
        }

        /*************************/
        // Querying
        /*************************/

        // Returns the open neighborhood of a vertex i, ie. N(i)
        public BitSet OpenNeighborhood(int i)
        {
            return _adjacencyMatrix[i];
        }

        // Returns the closed neighborhood of vertex i, ie. N[i]
        public BitSet ClosedNeighborhood(int i)
        {
            return _adjacencyMatrix[i] + i;
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
            return _adjacencyMatrix[i].Count;
        }

        /*************************/
        // File IO
        /*************************/

        // Outputs a graph using the DGF format
        public void Write(TextWriter writer)
        {
            writer.WriteLine("p edges {0}", Size);
            foreach (int v in Vertices)
                foreach (int w in OpenNeighborhood(v))
                    if (v < w)  // avoids printing edges twice
                        writer.WriteLine("e {0} {1}", v + 1, w + 1);
        }


        // Parses a file of DGF format, from which we can build a graph
        public static Graph Read(TextReader reader)
        {
            string line;
            int nVertices = 0;

            // First we parse the number of vertices
            while ((line = reader.ReadLine()) != null)
            {
                // Skip everything until we find the first useful line
                if (!line.StartsWith("p ")) continue;

                // First line always reads 'p edges {n} {e}'
                string[] def = line.Split();
                nVertices = int.Parse(def[2]); // number of vertices
                break;
            }

            // Initialize the graph
            Graph graph = new Graph(nVertices);

            Dictionary<int, int> identifiers = new Dictionary<int, int>();
            int counter = 0;
            bool renamed = false;

            while ((line = reader.ReadLine()) != null)
            {
                // Skip comments
                if (line.StartsWith("c ")) continue;

                // If a line starts with an n it means we do not work with integers [1,...,n], but we use an arbitrary set of integers
                // This means we need to keep track of which edges we should create internally, since we use [0,...,n)
                if (line.StartsWith("n "))
                {
                    renamed = true;
                    string[] node = line.Split();
                    int id = int.Parse(node[1]);    // actual identifier
                    identifiers[id] = counter++;      // counter will be the internal identifier
                }

                // Parsing of the edges
                if (line.StartsWith("e "))
                {
                    string[] edge = line.Split(' ');
                    int i = int.Parse(edge[1]);
                    int j = int.Parse(edge[2]);

                    // If there were arbitrary numbers used, look them up to find what we will use internally
                    if (renamed)
                    {
                        i = identifiers[i];
                        j = identifiers[j];
                    }
                    // If no other identifiers are given, we have only have to an element
                    // DGF files use the range [1...n], while internally we use [0...n), thus a simple minus 1 will suffice
                    else
                    {
                        i--;
                        j--;
                    }

                    // Create the edge
                    graph.Connect(i, j);
                }
            }
            
            return graph;
        }

        public Graph Clone ()
        {
            return new Graph(this);
        }
    }
}

