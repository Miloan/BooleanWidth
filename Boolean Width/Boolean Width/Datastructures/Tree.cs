using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BooleanWidth.Datastructures
{
    public class Tree : IEnumerable
    {

        /*************************/
        // Basic attributes
        /*************************/

        // Number of nodes in the decomposition tree
        public int Size { get { return _contained.Count; } }

        // First node of the tree
        public BitSet Root { get; private set; }

        // List of BitSets, representing the nodes, in the tree
        private List<BitSet> _contained;

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
            _contained = new List<BitSet>();
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
                Root = node;
                _contained.Add(node);
                return;
            }

            // The parent of the node that we are currently insering should be the node X that this node is a subset of, and X is has the highest index so far
            BitSet parent = new BitSet(0, 0);
            for (int i = Size - 1; i >= 0; i--)
            {
                parent = _contained[i];
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

            _contained.Add(node);
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

            _contained.Add(node);
        }

        /*************************/
        // Enumerator
        /*************************/

        // Simple enumerator to enumerate through all the nodes in the tree
        public IEnumerator GetEnumerator()
        {
            return _contained.GetEnumerator();
        }

        public void Write(TextWriter writer)
        {
            foreach (BitSet set in this._contained)
            {
                writer.WriteLine(string.Join(" ", set.Select(i => i + 1)));
            }
        }


        public void WriteLatex(TextWriter writer, bool simple = true)
        {
            writer.WriteLine("\\documentclass[tikz,border=5]{standalone}");
            writer.WriteLine("\\usetikzlibrary{graphs,graphdrawing,arrows.meta}");
            writer.WriteLine("\\usegdlibrary{trees}");
            writer.WriteLine("\\begin{document}");
            writer.WriteLine("\\begin{tikzpicture}[>=Stealth]");
            writer.WriteLine("\\graph[binary tree layout]{");
            if (simple)
            {
                _recursionSimple(writer, Root);
            }
            else
            {
                _recursion(writer, Root);
            }
            writer.WriteLine("};");
            writer.WriteLine("\\end{tikzpicture}");
            writer.WriteLine("\\end{document}");
        }

        private void _recursion(TextWriter writer, BitSet node)
        {
            IList<string> str = new List<string>();
            int start = int.MaxValue;
            int prev = int.MaxValue;
            foreach (int item in node)
            {
                if (item - prev != 1)
                {
                    start = item;
                    str.Add(item.ToString());
                }
                else
                {
                    str[str.Count - 1] = start + "-" + item;
                }
                prev = item;
            }
            writer.Write("\"\\{" + String.Join(", ", str) + "\\}\"");
            BitSet child;
            if (LeftChild.TryGetValue(node, out child))
            {
                writer.WriteLine("-> {");
                _recursion(writer, child);
                writer.WriteLine(",");
                _recursion(writer, RightChild[node]);
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine();
            }
        }

        private void _recursionSimple(TextWriter writer, BitSet node)
        {
            if (node.Count == 1)
            {
                writer.WriteLine(node.First());
            }
            else
            {
                writer.WriteLine("\"\"");
                writer.WriteLine("-> {");
                _recursionSimple(writer, LeftChild[node]);
                writer.WriteLine(",");
                _recursionSimple(writer, RightChild[node]);
                writer.WriteLine("}");
            }
        }
    }
}