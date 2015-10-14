using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BooleanWidth.Datastructures.Decompositions;

namespace BooleanWidth.Datastructures
{
    class BooleanChain
    {
        public BooleanChain Previous { get; private set; }

        public Graph Graph { get; private set; }

        public int Vertex { get; private set; }
        public BitSet Right { get; private set; }

        public Set<BitSet> Neighborhoods { get; private set; }

        public long MaxNeighborhoodSize { get; private set; }

        public double BooleanWidth => Math.Log(MaxNeighborhoodSize, 2);

        // Calculates the booleanwidth in a simple straightforward way by keeping track of the maximum number of distinct neighborhoods found so far
        private void CalculateBooleanWidth()
        {
            Set<BitSet> neighborhoods;
            if (Previous == null)
            {
                neighborhoods = new Set<BitSet> { new BitSet(0, Graph.Size) };
                Right = Graph.Vertices;
                MaxNeighborhoodSize = int.MinValue;
            }
            else
            {
                neighborhoods = Previous.Neighborhoods;
                MaxNeighborhoodSize = Previous.MaxNeighborhoodSize;
                Right = Previous.Right;
            }
            
            BitSet nv = Graph.OpenNeighborhood(this.Vertex) * Right;

            Neighborhoods = new Set<BitSet>();
            // We store the neighborhoods in a dictionary so that duplicates are automatically removed
            foreach (BitSet neighborhood in neighborhoods)
            {
                Neighborhoods.Add(neighborhood - this.Vertex);
                Neighborhoods.Add((neighborhood - this.Vertex) + nv);
            }

            MaxNeighborhoodSize = Math.Max(MaxNeighborhoodSize, Neighborhoods.Count);
            Right = Right - Vertex;
        }

        public BooleanChain(Graph graph, int vertex)
        {
            this.Graph = graph;
            this.Vertex = vertex;
            this.CalculateBooleanWidth();
        }

        public BooleanChain(BooleanChain chain, int vertex)
        {
            this.Previous = chain;
            this.Graph = chain.Graph;
            this.Vertex = vertex;
            this.CalculateBooleanWidth();
        }

        public static IEnumerable<BooleanChain> FromGraph(Graph graph, BitSet bitSet)
        {
            foreach (int vertex in bitSet)
            {
                yield return new BooleanChain(graph, vertex);
            }
        }
        
        public static BooleanChain DepthFirstSearch(BitSet items, long min, long max, params BooleanChain[] chains)
        {
            Stack<BooleanChain> stack = new Stack<BooleanChain>();
            foreach (BooleanChain chain in chains)
            {
                stack.Push(chain);
            }

            BooleanChain best = null;
            while (stack.Count > 0)
            {
                BooleanChain chain = stack.Pop();
                if (chain.MaxNeighborhoodSize <= max)
                {
                    bool empty = true;
                    foreach (BooleanChain next in (chain.Right * items)
                        .Select(i => new BooleanChain(chain, i))
                        .OrderByDescending(c => c.MaxNeighborhoodSize))
                    {
                        empty = false;
                        if (next.MaxNeighborhoodSize <= max)
                        {
                            stack.Push(next);
                        }
                    }
                    if (empty)
                    {
                        best = chain;
                        max = best.MaxNeighborhoodSize - 1;
                        if (max < min)
                        {
                            break;
                        }
                    }
                }
            }
            return best;
        }

        public static BooleanChain BestFirstSearch(BitSet items, long max, params BooleanChain[] chains)
        {
            PriorityQueue<double, BooleanChain> queue = new PriorityQueue<double, BooleanChain>(Comparer<double>.Default);
            foreach (BooleanChain item in chains)
            {
                if (item.MaxNeighborhoodSize <= max)
                {
                    queue.Enqueue(item.BooleanWidth * (item.Right * items).Count, item);
                }
            }

            BooleanChain chain;
            while (queue.TryDequeue(out chain))
            {
                BitSet bitSet = chain.Right * items;

                bool empty = true;
                foreach (int item in bitSet)
                {
                    empty = false;
                    BooleanChain next = new BooleanChain(chain, item);
                    if (next.MaxNeighborhoodSize <= max)
                    {
                        queue.Enqueue(next.BooleanWidth * (next.Right * items).Count, next);
                    }
                }

                if (empty)
                {
                    return chain;
                }
            }

            return null;
        }

        public static explicit operator LinearDecomposition(BooleanChain chain)
        {
            Stack<int> stack = new Stack<int>();
            BooleanChain parent = chain;
            while (parent != null)
            {
                stack.Push(parent.Vertex);
                parent = parent.Previous;
            }

            return new LinearDecomposition(chain.Graph, stack.ToList());
        }
    }
}
