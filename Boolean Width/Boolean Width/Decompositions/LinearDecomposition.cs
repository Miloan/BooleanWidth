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

namespace Boolean_Width
{
    public class LinearDecomposition
    {
        // Ordering of the decomposition
        public readonly List<int> Sequence;

        // Graph belonging to this decompositon
        public readonly Graph Graph;

        // Boolean-width belonging to this decomposition
        public double BooleanWidth { get { return Math.Log(MaxNeighborhoodSize, 2); } }

        // Boolean-dimension of this decomposition, which we possibly have to calculate if not calculated before
        private long maxNeighborhoodSize;
        public long MaxNeighborhoodSize
        {
            get
            {
                if (maxNeighborhoodSize == 0)
                    maxNeighborhoodSize = CalculateBooleanWidth();
                return maxNeighborhoodSize;
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
                max = Math.Max(max, CC_MIS.Compute(bg));
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

        public void ToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine("c max|UN(A)| = {0}, boolw = {1}", MaxNeighborhoodSize, BooleanWidth);
            foreach (int v in Sequence)
                sw.WriteLine(v + 1);    // The +1 is needed because internally we work with the range [0...n) instead of (0...n]
            sw.Close();
        }

        // Creates a regular boolean decomposition out of a linear boolean decomposition
        public void ToBDCFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine("c max|UN(A)| = {0}, boolw = {1}", MaxNeighborhoodSize, BooleanWidth);

            StringBuilder sb = new StringBuilder();

            // All vertices that have to be printed, put together as one long string, ie. "1 2 3 4 5 6 7"
            foreach (int v in Sequence)
            {
                sb.Append(v + 1);
                sb.Append(" ");
            }

            int skip = 0;
            for (int i = 0; i < Sequence.Count; i++)
            {
                // Parent set of the vertex we are splitting off
                sw.WriteLine(sb.ToString(skip, sb.Length - skip - 1));

                // Ávoids printing the last vertex twice
                if (i == Sequence.Count - 1) 
                    break;

                // Actual vertex that we print
                string vertex = (Sequence[i] + 1).ToString();
                sw.WriteLine(vertex);

                // +1 because of " " in each entry of the stringbuilder
                skip += vertex.Length + 1;                  
            }
            sw.Close();
        }
    }
}
