/*************************/
// Frank van Houten, 2014 - 2015
//
// A representative table consists of keyvalue pairs, where each entry uses a cut as a key (thus a bitset),
// and a list of representatives belonging to that cut as a value.
// Construct representatives for all sigma,rho problems, regardless of their d-value.
/*************************/

using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public class RepresentativeTable
    {
        // Table[cut] will return the list of representatives at a given cut
        private Dictionary<BitSet, RepresentativeList> Table;

        // Maximum dimension that occurs in this entire table, in other words the size of the longest list of representatives across all cuts
        public long MaxDimension { get; private set; }

        // Indexer
        public RepresentativeList this[BitSet cut]
        {
            get { return Table[cut]; }
        }

        // Constructor for creating a new representative table. We directly fill the table depending on the given sequence.
        public RepresentativeTable(Graph graph, Tree tree)
        {
            // Initialize the table
            Table = new Dictionary<BitSet, RepresentativeList>();

            Queue<BitSet> queue = new Queue<BitSet>();
            queue.Enqueue(tree.Root);
            Table[new BitSet(0, graph.Size)] = new RepresentativeList();

            int i = 0;

            while (queue.Count != 0)
            {
                BitSet node = queue.Dequeue();

                FillTable(graph, node);
                FillTable(graph, graph.Vertices - node);

                if (tree.LeftChild.ContainsKey(node))
                {
                    queue.Enqueue(tree.LeftChild[node]);
                }
                if (tree.RightChild.ContainsKey(node))
                {
                    queue.Enqueue(tree.RightChild[node]);
                }
            }
        }

        // Implementation of Algorithm 1 of 'Fast dynamic programming for locally checkable vertex subset and vertex partitioning problems' (Bui-Xuan et al.)
        // Used to compute the representatives and their corresponding dNeighborhoods for a given node of the decomposition tree
        private void FillTable(Graph graph, BitSet cut)
        {
            int n = graph.Size;
            BitSet _cut = graph.Vertices - cut;

            // Lists of representatives that we keep track of on each level of the algorithm
            RepresentativeList representatives = new RepresentativeList();
            RepresentativeList lastLevel = new RepresentativeList();

            // Initial insertions, the empty set always has the empty neighborhood set as a representative initially
            dNeighborhood dInitial = new dNeighborhood(_cut);
            representatives.Update(new BitSet(0, n), dInitial);
            lastLevel.Update(new BitSet(0, n), dInitial);


            while (lastLevel.Count > 0)
            {
                RepresentativeList nextLevel = new RepresentativeList();
                foreach (BitSet r in lastLevel)
                {
                    foreach (int v in cut)
                    {
                        // avoid that r_ = r, since we already saved all sets of that size
                        if (r.Contains(v))
                            continue; 

                        BitSet r_ = r + v;
                        dNeighborhood dn = representatives.GetNeighborhood(r).CopyAndUpdate(graph, v);

                        if (!representatives.ContainsNeighborhood(dn) && !dn.Equals(representatives.GetNeighborhood(r)))
                        {
                            nextLevel.Update(r_, dn);
                            representatives.Update(r_, dn);
                        }
                    }
                }

                // Update the values for the next iteration
                lastLevel = nextLevel;
            }

            // Save the representatives at the current cut in the table
            Table[cut.Copy()] = representatives;

            // Save the maximum size that we encounter during all iterations; this will be the boolean dimension of the graph is d = 1.
            MaxDimension = Math.Max(MaxDimension, representatives.Count);
        }
    }
}

