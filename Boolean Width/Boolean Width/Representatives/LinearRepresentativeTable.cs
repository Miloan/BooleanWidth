/*************************/
// Frank van Houten, 2014 - 2015
//
// A representative table consists of keyvalue pairs, where each entry uses a cut as a key (thus a bitset),
// and a list of representatives belonging to that cut as a value.
// This only construct representatives for sigma rho problems with d=1, i.e., regular representatives and is an implementation of the faster algorithm to compute representatives
// given in the thesis.
/*************************/

using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public class LinearRepresentativeTable
    {
        // Table[cut] will return the list of representatives at a given cut
        private Dictionary<BitSet, LinearRepresentativeList> Table;

        // Maximum dimension that occurs in this entire table, in other words the size of the longest list of representatives across all cuts
        public long MaxDimension { get; private set; }

        // Indexer
        public LinearRepresentativeList this[BitSet cut]
        {
            get { return Table[cut]; }
        }

        // Constructor for creating a new representative table. We directly fill the table depending on the given sequence.
        public LinearRepresentativeTable(Graph graph, List<int> sequence)
        {
            // Initialize the table
            Table = new Dictionary<BitSet, LinearRepresentativeList>();

            int n = graph.Size;

            // Save these initial representatives for the cut(empty, V(G))
            LinearRepresentativeList reps = new LinearRepresentativeList();
            reps.Update(new BitSet(0, n), new BitSet(0, n));
            Table[new BitSet(0, n)] = reps;
            Table[graph.Vertices] = reps;

            FillTable(graph, sequence);

            List<int> reverseSequence = new List<int>();
            for (int i = sequence.Count - 1; i >= 0; i--)
                reverseSequence.Add(sequence[i]);

            FillTable(graph, reverseSequence);
        }

        private void FillTable(Graph graph, List<int> sequence)
        {
            int n = graph.Size;

            // Processed vertices
            BitSet left = new BitSet(0, n);

            // Unprocessed vertices
            BitSet right = graph.Vertices;

            // Lists of representatives that we keep track of on each level of the algorithm
            LinearRepresentativeList reps = new LinearRepresentativeList();

            // Initial insertions, the empty set always has the empty neighborhood set as a representative initially
            reps.Update(new BitSet(0, n), new BitSet(0, n));

            for (int i = 0; i < sequence.Count; i++)
            {
                /// We give v the possibility to be a representative instead of being contained in neighborhoods
                int v = sequence[i];

                // Actually move v from one set to the other set
                left += v;
                right -= v;

                // We don't want to disturb any pointers so we create new empty datastructures to save everything for the next iteration
                LinearRepresentativeList nextReps = new LinearRepresentativeList();

                // We are iterating over all representatives saved inside the list of the previous step. For each entry there are only two possibilities to create a new neighborhood
                foreach (BitSet representative in reps)
                {
                    // Case 1: The neighborhood possibly contained v (thus v has to be removed), but is still valid
                    BitSet neighborhood = reps.GetNeighborhood(representative) - v;
                    nextReps.Update(representative, neighborhood);

                    // Case 2: The union of the old neighborhood, together with v's neighborhood, creates a new entry. The representative is uniond together with v and saved.
                    BitSet representative_ = representative + v;
                    BitSet neighborhood_ = neighborhood + (graph.OpenNeighborhood(v) * right);
                    nextReps.Update(representative_, neighborhood_);
                }

                // Update the values for the next iteration
                reps = nextReps;

                // Save the maximum size that we encounter during all iterations; this will be the boolean dimension of the graph.
                MaxDimension = Math.Max(MaxDimension, reps.Count);

                // Save the representatives at the current cut in the table
                Table[left] = reps;
            }
        }
    }
}
