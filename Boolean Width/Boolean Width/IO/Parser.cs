/*************************/
// Frank van Houten, 2014 - 2015
//
// Class with parsers to parse files of the DGF fileformat, and parsers to retreive saved linear decompositions.
/*************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace Boolean_Width
{
	public static class Parser
	{
        // Parses a decomposition given the filename of the decomposition and the graph files.
        public static Decomposition ParseDecomposition(string decompositionFilename, string graphFilename)
        {
            Graph graph = ParseGraph(graphFilename);
            StreamReader sr = new StreamReader(decompositionFilename);
            string line;
            Tree tree = new Tree();

            // Each line is simply an integer, which is the sequence of the linear decomposition
            while ((line = sr.ReadLine()) != null)
            {
                // Skip comments
                if (line.StartsWith("c ")) continue;

                string[] s = line.Trim().Split(' ');
                BitSet node = new BitSet(0, graph.Size);
                foreach (string vertex in s)
                    node.Add(int.Parse(vertex) - 1); // -1 because internally we work with [0,...,n)
                tree.Insert(node);
            }

            return new Decomposition(graph, tree);
        }

        // Parses a linear decomposition given the filename of the decomposition and the graph files.
        public static LinearDecomposition ParseLinearDecomposition(string decompositionFilename, string graphFilename)
        {
            StreamReader sr = new StreamReader(decompositionFilename);
            string line;
            List<int> sequence = new List<int>();

            // Each line is simply an integer, which is the sequence of the linear decomposition
            while ((line = sr.ReadLine()) != null)
            {
                // Skip comments
                if (line.StartsWith("c ")) continue; 

                int i;
                int.TryParse(line, out i);
                sequence.Add(i - 1); // -1 because we work internally with [0...n) instead of (0...n]
            }

            Graph graph = ParseGraph(graphFilename);

            return new LinearDecomposition(graph, sequence);
        }

        // Parses a file of DGF format, from which we can build a graph
		public static Graph ParseGraph (string filename)
		{
			StreamReader sr = new StreamReader (filename);

			string line;
			int nVertices = 0;

			// First we parse the number of vertices
			while ((line = sr.ReadLine ()) != null) 
			{
                // Skip everything until we find the first useful line
				if (!line.StartsWith ("p ")) continue;  

                // First line always reads 'p edges {n} {e}'
				string[] def = line.Split ();
				nVertices = int.Parse (def[2]); // number of vertices
				break;
			}

            // Initialize the graph
			Graph graph = new Graph (nVertices);

			Dictionary<int, int> identifiers = new Dictionary <int, int>();
			int counter = 0;
			bool renamed = false;

			while ((line = sr.ReadLine ()) != null) 
			{
				// Skip comments
				if (line.StartsWith ("c ")) continue;

                // If a line starts with an n it means we do not work with integers [1,...,n], but we use an arbitrary set of integers
                // This means we need to keep track of which edges we should create internally, since we use [0,...,n)
				if (line.StartsWith ("n ")) 
				{
					renamed = true;
					string[] node = line.Split();
					int id = int.Parse(node[1]);    // actual identifier
					identifiers[id] = counter++;      // counter will be the internal identifier
				}

                // Parsing of the edges
				if (line.StartsWith ("e "))
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
					graph.Connect (i, j);
				}
			}

			sr.Close ();
			return graph;
		}
	}
}

