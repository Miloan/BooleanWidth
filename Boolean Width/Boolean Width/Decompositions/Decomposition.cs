/*************************/
// Frank van Houten, 2014 - 2015
//
// Wrapper class for decompositions, which consist of a graph and a the actual decomposition tree.
/*************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Boolean_Width
{
    public class Tree : IEnumerable
    {

        /*************************/
        // Basic attributes
        /*************************/

        // Number of nodes in the decomposition tree
        public int Size { get { return Contained.Count; } }

        // First node of the tree
        public BitSet Root { get { return root.Copy(); } }
        private BitSet root;

        // List of BitSets, representing the nodes, in the tree
        private List<BitSet> Contained;

        // Saves the parent of a node, given that node
        public Dictionary<BitSet, BitSet> Parent { get; private set; }

        // Saves the left child of a node, given that node
        public Dictionary<BitSet, BitSet> LeftChild { get; private set; }

        // Saves the right child of a node, given that node
        public Dictionary<BitSet, BitSet> RightChild { get; private set; }

        /*************************/
        // Constructor
        /*************************/
        public Tree()
        {
            Contained = new List<BitSet>();
            Parent = new Dictionary<BitSet, BitSet>();
            LeftChild = new Dictionary<BitSet, BitSet>();
            RightChild = new Dictionary<BitSet, BitSet>();
        }

        /*************************/
        // Basic operations
        /*************************/

        // Inserting works correctly because we assume that a node that gets newly inserted will not be in the tree already
        // By definition of boolean decompositions this is redundant and the node should be removed from the decomposition tree
        public void Insert(BitSet node)
        {
            if (Parent.ContainsKey(node))
                throw new Exception("This node already exists in the decomposition tree");

            // Check if this is the first node that we add to the collection
            if (Size == 0)
            {
                root = node;
                Contained.Add(node);
                return;
            }

            // The parent of the node that we are currently insering should be the node X that this node is a subset of, and X is has the highest index so far
            BitSet parent = new BitSet(0, 0);
            for (int i = Size - 1; i >= 0; i--)
            {
                parent = Contained[i];
                if (node.IsSubsetOf(parent))
                    break;
            }

            Parent[node] = parent;

            // The node will always be a leftchild if the previously inserted node had an even number, and vice versa
            // This assumes that we always add children of a node directly after each other
            if (Size % 2 == 0)
                RightChild[parent] = node;
            else
                LeftChild[parent] = node;

            Contained.Add(node);
        }

        // Inserting a node with its parent saves us the time of finding the parent
        public void InsertWithParent(BitSet node, BitSet parent)
        {
            Parent[node] = parent;

            if (Size % 2 == 0)
            {
                if (RightChild.ContainsKey(parent))
                    throw new Exception("Inserted parent already has a leftchild");
                RightChild[parent] = node;
            }
            else
            {
                if (LeftChild.ContainsKey(parent))
                    throw new Exception("Inserted parent already has a leftchild");
                LeftChild[parent] = node;
            }

            Contained.Add(node);
        }

        /*************************/
        // Enumerator
        /*************************/

        // Simple enumerator to enumerate through all the nodes in the tree
        public IEnumerator GetEnumerator()
        {
            return Contained.GetEnumerator();
        }

        public void ToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            StringBuilder sb = new StringBuilder();
            foreach (BitSet set in this)
            {
                sb.Clear();
                foreach (int i in set)
                {
                    sb.Append(i + 1);
                    sb.Append(" ");
                }
                sb.Remove(sb.Length - 1, 1);
                sw.WriteLine(sb.ToString());
            }
            sw.Close();
        }
    }

    public class Decomposition
    {

        /*************************/
        // Basic attributes
        /*************************/
        // Ordering of the decomposition
        public readonly Tree Tree;

        // Graph belonging to this decompositon
        public readonly Graph Graph;

        // Boolean-width belonging to this decomposition, by definition the log of the boolean-dimension
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


        /*************************/
        // Constructor
        /*************************/
        public Decomposition(Graph graph, Tree tree)
        {
            Tree = tree;
            Graph = graph;
        }

        /*************************/
        // General methods
        /*************************/

        // Calculating the boolean dimension is done by computing all representatives
        private long CalculateBooleanWidth()
        {
            dNeighborhood.Initialize(new IndependentSet(2));
            RepresentativeTable cuts = new RepresentativeTable(Graph, Tree);
            return cuts.MaxDimension;
        }

        /*************************/
        // File IO
        /*************************/

        // Outputs the decomposition in BDC file format
        public void ToFile(string filename)
        {
            Tree.ToFile(filename);
        }
        public static Decomposition FromLinearDecomposition(LinearDecomposition ld)
        {
            Tree tree = new Tree();
            tree.Insert(ld.Graph.Vertices);
            BitSet parent = ld.Graph.Vertices;
            foreach (int item in ld.Sequence.Take(ld.Sequence.Count - 1))
            {
                tree.InsertWithParent(parent * item, parent);
                tree.InsertWithParent(parent - item, parent);
                parent = parent - item;
            }
            return new Decomposition(ld.Graph, tree)
            {
                // TODO: speed up using this
                //maxNeighborhoodSize = ld.MaxNeighborhoodSize
            };
        }
    }
}
