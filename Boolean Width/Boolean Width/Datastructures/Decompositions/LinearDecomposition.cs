/*************************/
// Frank van Houten, 2014 - 2015
//
// Wrapper class for decompositions, which consist of a graph and a sequence of vertices that is the actual ordering
// of the linear decomposition.
/*************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BooleanWidth.Algorithms.Maximal_IS_Counting;

namespace BooleanWidth.Datastructures.Decompositions
{
    public class LinearDecomposition : IDecomposition
    {
        // Ordering of the decomposition
        public readonly List<int> Sequence;

        // Graph belonging to this decompositon
        public Graph Graph { get; private set; }

        // Boolean-width belonging to this decomposition
        public double BooleanWidth { get { return Math.Log(MaxNeighborhoodSize, 2); } }

        // Boolean-dimension of this decomposition, which we possibly have to calculate if not calculated before
        private long _maxNeighborhoodSize;
        public long MaxNeighborhoodSize
        {
            get
            {
                if (_maxNeighborhoodSize == 0)
                    _maxNeighborhoodSize = CalculateBooleanWidth();
                return _maxNeighborhoodSize;
            }
        }
        
        // Basic constructor
        public LinearDecomposition(Graph graph, List<int> sequence)
        {
            Sequence = sequence;
            Graph = graph;
        }

        // Calculates the booleanwidth in a simple straightforward way by keeping track of the maximum number of distinct neighborhoods found so far
        private long CalculateBooleanWidth()
        {
            int max = int.MinValue;
            Set<BitSet> neighborhoods = new Set<BitSet>();

            BitSet right = Graph.Vertices;

            // Initially we only have the empty neighborhood
            neighborhoods.Add(new BitSet(0, Graph.Size));

            // We process vertices of the sequence one by one and save their effect on all saved neighborhoods
            foreach (int v in Sequence)
            {
                Set<BitSet> next = new Set<BitSet>();
                BitSet nv = Graph.OpenNeighborhood(v) * right;

                // We store the neighborhoods in a dictionary so that duplicates are automatically removed
                foreach (BitSet neighborhood in neighborhoods)
                {
                    next.Add(neighborhood - v);
                    next.Add((neighborhood - v) + nv);
                }
                neighborhoods = next;
                max = Math.Max(max, neighborhoods.Count);
                right -= v;
            }
            return max;
        }

        // Alternative way of calculating the boolean width of a decomposition by counting the number of maximal independent sets in bipartite graphs,
        // constructed for each cut of the decomposition
        private long CalculateBooleanWidthBiPartite()
        {
            BitSet left = new BitSet(0, Graph.Size);
            BitSet right = Graph.Vertices;
            long max = int.MinValue;

            foreach (int v in Sequence)
            {
                left += v;
                right -= v;
                // Construct the bipartite graph
                BipartiteGraph bg = new BipartiteGraph(Graph, left, right);

                // Count the number of maximal independent sets in this bipartite graph
                max = Math.Max(max, CcMis.Compute(bg));
            }
            return max;
        }

        /*************************/
        // Print
        /*************************/
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Sequence.Count);
            foreach (int v in Sequence)
            {
                sb.Append(v + " ");
            }
            return sb.ToString();
        }

        /*************************/
        // File IO
        /*************************/

        public void Write(TextWriter writer)
        {
            writer.WriteLine("c max|UN(A)| = {0}, boolw = {1}", MaxNeighborhoodSize, BooleanWidth);
            foreach (int v in Sequence)
                writer.WriteLine(v + 1);    // The +1 is needed because internally we work with the range [0...n) instead of (0...n]
        }

        // Parses a linear decomposition given the filename of the decomposition and the graph files.
        public static LinearDecomposition Read(TextReader reader, Graph graph)
        {
            string line;
            List<int> sequence = new List<int>();

            // Each line is simply an integer, which is the sequence of the linear decomposition
            while ((line = reader.ReadLine()) != null)
            {
                // Skip comments
                if (line.StartsWith("c ")) continue;

                int i;
                int.TryParse(line, out i);
                sequence.Add(i - 1); // -1 because we work internally with [0...n) instead of (0...n]
            }

            return new LinearDecomposition(graph, sequence);
        }
    }
}
