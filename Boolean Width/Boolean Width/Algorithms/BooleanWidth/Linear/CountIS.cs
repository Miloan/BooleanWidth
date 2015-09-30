/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of a linear version of the algorithm to count the number of independent sets in a graph.
/*************************/


using System;
using System.Collections.Generic;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;
using BooleanWidth.Datastructures.Representatives;

namespace BooleanWidth.Algorithms.BooleanWidth.Linear
{
    public static class CountIs
    {
        class LookupTable
        {
            private Dictionary<BitSet, int>[] _data;
            public LookupTable(int n)
            {
                _data = new Dictionary<BitSet, int>[n + 1];

                for (int i = 0; i <= n; i++)
                    _data[i] = new Dictionary<BitSet, int>();
            }

            public int this[BitSet representative, int k]
            {
                get
                {
                    if (!_data[k].ContainsKey(representative))
                        return 0;
                    return _data[k][representative];
                }
                set
                {
                    _data[k][representative] = value;
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

                LinearRepresentativeList lRw = cuts[left];

                LinearRepresentativeList lRa = cuts[left - v];

                LookupTable newTable = new LookupTable(n);

                foreach (BitSet ra in lRa)
                {
                    BitSet nra = graph.Neighborhood(ra) * right;
                    BitSet rw = lRw.GetRepresentative(nra);

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
                    if (lRa.GetNeighborhood(ra).Contains(v))
                        continue;

                    //------------

                    // add {v} to the independent set
                    BitSet nrav = graph.Neighborhood(ra + v) * right;
                    BitSet rwv = lRw.GetRepresentative(nrav);

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
