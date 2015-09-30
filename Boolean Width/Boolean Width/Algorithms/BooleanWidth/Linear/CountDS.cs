/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of a linear version of the algorithm to count the number of dominating sets in a graph.
/*************************/

using System;
using System.Collections.Generic;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;
using BooleanWidth.Datastructures.Representatives;

namespace BooleanWidth.Algorithms.BooleanWidth.Linear
{
    public static class CountDs
    {
        class LookupTable
        {
            private Dictionary<Tuple<BitSet, BitSet>, int>[] _data;
            public LookupTable(int n)
            {
                _data = new Dictionary<Tuple<BitSet, BitSet>, int>[n + 1];

                for (int i = 0; i < _data.Length; i++)
                    _data[i] = new Dictionary<Tuple<BitSet, BitSet>, int>();
            }

            public int this[BitSet inner, BitSet outer, int k]
            {
                get
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inner, outer);
                    if (!_data[k].ContainsKey(t))
                        return 0;
                    return _data[k][t];
                }
                set
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inner, outer);
                    _data[k][t] = value;
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

            int l = sequence[0];
            BitSet leaf = new BitSet(0, n, new int[] { l });
            BitSet nleaf = new BitSet(0, n, new int[] { graph.OpenNeighborhood(l).First() });

            table[leaf, new BitSet(0, n), 1] = 1;
            table[leaf, nleaf, 1] = 1;
            table[new BitSet(0, n), nleaf, 0] = 1;

            left += l;
            right -= l;

            for (int i = 1; i < sequence.Count; i++)
            {
                int v = sequence[i];

                left += v;
                right -= v;

                LinearRepresentativeList lRw = cuts[left];
                LinearRepresentativeList LRw_ = cuts[right];

                LinearRepresentativeList lRa = cuts[left - v];
                LinearRepresentativeList LRa_ = cuts[right + v];

                LookupTable newTable = new LookupTable(n);

                foreach (BitSet outside in LRw_)
                {
                    foreach (BitSet inside in lRa)
                    {
                        BitSet nrw = graph.Neighborhood(outside) * (left - v);
                        BitSet ra_ = LRa_.GetRepresentative(nrw);

                        BitSet nra = graph.Neighborhood(inside) * right;
                        BitSet rw = lRw.GetRepresentative(nra);

                        BitSet ra = inside;
                        BitSet rw_ = outside;

                        BitSet raw = inside + outside;
                        BitSet nraw = graph.Neighborhood(raw);
                        if (nraw.Contains(v))  // this means rb_ exists ==> multiplier is equal to 1
                        {
                            for (int ka = 0; ka < n; ka++)
                            {
                                newTable[rw, rw_, ka] = newTable[rw, rw_, ka] + table[ra, ra_, ka];
                            }
                        }

                        //--------

                        nrw = graph.Neighborhood(outside + v) * (left - v);
                        ra_ = LRa_.GetRepresentative(nrw);

                        nra = graph.Neighborhood(inside + v) * right;
                        rw = lRw.GetRepresentative(nra);

                        for (int ka = 0; ka < n; ka++)
                        {
                            newTable[rw, rw_, ka + 1] = newTable[rw, rw_, ka + 1] + table[ra, ra_, ka];
                        }
                    }
                }

                table = newTable;
            }

            return;
        }
    }
}
