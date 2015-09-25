/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of a linear version of the algorithm to count the number of independent sets in a graph.
/*************************/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boolean_Width
{
    public static class CountIS
    {
        class LookupTable
        {
            private Dictionary<BitSet, int>[] Data;
            public LookupTable(int n)
            {
                Data = new Dictionary<BitSet, int>[n + 1];

                for (int i = 0; i <= n; i++)
                    Data[i] = new Dictionary<BitSet, int>();
            }

            public int this[BitSet representative, int k]
            {
                get
                {
                    if (!Data[k].ContainsKey(representative))
                        return 0;
                    return Data[k][representative];
                }
                set
                {
                    Data[k][representative] = value;
                }
            }
        }

        public static void Compute(LinearDecomposition decomposition)
        {
            Graph graph = decomposition.Graph;
            List<int> sequence = decomposition.Sequence;
            int n = graph.Size;

            BitSet right = graph.Vertices;
            BitSet left = new BitSet(0, n);

            LookupTable table = new LookupTable(n);
            LinearRepresentativeTable cuts = new LinearRepresentativeTable(graph, sequence);

            table[new BitSet(0, n), 0] = 1;
            table[new BitSet(0, n), 1] = 0;

            for (int i = 0; i < sequence.Count; i++)
            {
                int v = sequence[i];

                left += v;
                right -= v;

                LinearRepresentativeList LRw = cuts[left];

                LinearRepresentativeList LRa = cuts[left - v];

                LookupTable newTable = new LookupTable(n);

                foreach (BitSet ra in LRa)
                {
                    BitSet nra = graph.Neighborhood(ra) * right;
                    BitSet rw = LRw.GetRepresentative(nra);

                    int maxValue = int.MinValue;
                    int limit = (left - v).Count;
                    for (int k = 0; k <= limit; k++)
                        if (table[ra, k] > 0)
                            maxValue = Math.Max(maxValue, k);

                    for (int j = 0; j <= maxValue; j++)
                    {
                        newTable[rw, j] = newTable[rw, j] + table[ra, j];
                    }

                    //------------

                    // ra + {v} is not a valid independent set
                    if (LRa.GetNeighborhood(ra).Contains(v))
                        continue;

                    //------------

                    // add {v} to the independent set
                    BitSet nrav = graph.Neighborhood(ra + v) * right;
                    BitSet rwv = LRw.GetRepresentative(nrav);

                    for (int j = 0; j <= maxValue; j++)
                    {
                        newTable[rwv, j + 1] = newTable[rwv, j + 1] + table[ra, j];
                    }
                }

                table = newTable;
            }

            return;
        }
    }
}
