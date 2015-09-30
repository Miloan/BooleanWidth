/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of a linear version of the dominating set problem. Basically a less general version of the generic solver.
/*************************/

using System;
using System.Collections.Generic;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;
using BooleanWidth.Datastructures.Representatives;

namespace BooleanWidth.Algorithms.BooleanWidth.Linear
{
    public static class LinearMinimumDs
    {
        class LookupTable
        {
            private Dictionary<Tuple<BitSet, BitSet>, int> _data;

            public LookupTable()
            {
                _data = new Dictionary<Tuple<BitSet, BitSet>, int>();
            }

            public int this[BitSet inside, BitSet outside]
            {
                get 
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inside, outside);
                    if (!_data.ContainsKey(t))
                        return int.MaxValue;
                    return _data[t];
                }
                set
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inside, outside);
                    _data[t] = value;
                }
            }

        }

        public static int Compute(LinearDecomposition decomposition)
        {
            Graph graph = decomposition.Graph;
            int n = graph.Size;
            List<int> sequence = decomposition.Sequence;
            BitSet left = new BitSet(0, graph.Size);
            BitSet right = graph.Vertices;
            BitSet vg = graph.Vertices;

            LinearRepresentativeTable cuts = new LinearRepresentativeTable(graph, sequence);
            LookupTable table = new LookupTable();

            // first initialize the very first leaf node
            int l = sequence[0];
            left += l;
            right -= l;

            // Base cases
            BitSet leaf = new BitSet(0, n, new int[] { l });
            BitSet nleaf = new BitSet(0, n, new int[] { graph.OpenNeighborhood(l).First() });

            table[new BitSet(0, n), new BitSet(0, n)] = int.MaxValue;
            table[leaf, new BitSet(0, n)] = 1;
            table[leaf, nleaf] = 1;
            table[new BitSet(0, n), nleaf] = 0;
 
            for (int i = 1; i < sequence.Count; i++)
            {
                int v = sequence[i];

                left += v;
                right -= v;

                LinearRepresentativeList lRw = cuts[left];
                LinearRepresentativeList LRw_ = cuts[right];

                LinearRepresentativeList lRa = cuts[left - v];
                LinearRepresentativeList LRa_ = cuts[right + v];

                LookupTable newTable = new LookupTable();

                foreach (BitSet outside in LRw_)
                {
                    foreach (BitSet inside in lRa)
                    {
                        BitSet nrw = graph.Neighborhood(outside) * (left - v);
                        BitSet ra = LRa_.GetRepresentative(nrw);

                        BitSet nra = graph.Neighborhood(inside) * right;
                        BitSet rw = lRw.GetRepresentative(nra);

                        int savedValue = newTable[rw, outside];
                        int newValue = table[inside, ra];

                        BitSet raw = inside + outside;
                        BitSet nraw = graph.Neighborhood(raw);
                        if (!nraw.Contains(v))
                            newValue = int.MaxValue;

                        int min = Math.Min(savedValue, newValue);
                        newTable[rw, outside] = min;

                        //--------

                        nrw = graph.Neighborhood(outside + v) * (left - v);
                        ra = LRa_.GetRepresentative(nrw);

                        nra = graph.Neighborhood(inside + v) * right;
                        rw = lRw.GetRepresentative(nra);

                        savedValue = newTable[rw, outside];
                        newValue = table[inside, ra];
                        newValue = newValue == int.MaxValue ? newValue : newValue + 1;
                        min = Math.Min(savedValue, newValue);
                        newTable[rw,  outside] = min;
                    }
                }

                table = newTable;
            }

            return table[new BitSet(0, graph.Size), new BitSet(0, graph.Size)];
        }
    }
}
