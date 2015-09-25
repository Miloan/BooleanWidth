/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of a linear version of the algorithm to count the number of dominating sets in a graph.
/*************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boolean_Width
{
    public static class CountDS
    {
        class LookupTable
        {
            private Dictionary<Tuple<BitSet, BitSet>, int>[] Data;
            public LookupTable(int n)
            {
                Data = new Dictionary<Tuple<BitSet, BitSet>, int>[n + 1];

                for (int i = 0; i < Data.Length; i++)
                    Data[i] = new Dictionary<Tuple<BitSet, BitSet>, int>();
            }

            public int this[BitSet inner, BitSet outer, int k]
            {
                get
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inner, outer);
                    if (!Data[k].ContainsKey(t))
                        return 0;
                    return Data[k][t];
                }
                set
                {
                    Tuple<BitSet, BitSet> t = new Tuple<BitSet, BitSet>(inner.Copy(), outer.Copy());
                    Data[k][t] = value;
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
            BitSet leaf = new BitSet(0, n) { l };
            BitSet nleaf = new BitSet(0, n) { graph.OpenNeighborhood(l).First() };

            table[leaf, new BitSet(0, n), 1] = 1;
            table[leaf, nleaf, 1] = 1;
            table[new BitSet(0, n), nleaf, 0] = 1;

            left.Add(l);
            right.Remove(l);

            for (int i = 1; i < sequence.Count; i++)
            {
                int v = sequence[i];

                left.Add(v);
                right.Remove(v);

                LinearRepresentativeList LRw = cuts[left];
                LinearRepresentativeList LRw_ = cuts[right];

                LinearRepresentativeList LRa = cuts[left - v];
                LinearRepresentativeList LRa_ = cuts[right + v];

                LookupTable newTable = new LookupTable(n);

                foreach (BitSet outside in LRw_)
                {
                    foreach (BitSet inside in LRa)
                    {
                        BitSet nrw_ = graph.Neighborhood(outside) * (left - v);
                        BitSet ra_ = LRa_.GetRepresentative(nrw_);

                        BitSet nra = graph.Neighborhood(inside) * right;
                        BitSet rw = LRw.GetRepresentative(nra);

                        BitSet ra = inside;
                        BitSet rw_ = outside;

                        BitSet raw_ = inside + outside;
                        BitSet nraw_ = graph.Neighborhood(raw_);
                        if (nraw_.Contains(v))  // this means rb_ exists ==> multiplier is equal to 1
                        {
                            for (int ka = 0; ka < n; ka++)
                            {
                                newTable[rw, rw_, ka] = newTable[rw, rw_, ka] + table[ra, ra_, ka];
                            }
                        }

                        //--------

                        nrw_ = graph.Neighborhood(outside + v) * (left - v);
                        ra_ = LRa_.GetRepresentative(nrw_);

                        nra = graph.Neighborhood(inside + v) * right;
                        rw = LRw.GetRepresentative(nra);

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
