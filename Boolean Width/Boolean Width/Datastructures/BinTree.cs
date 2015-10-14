using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanWidth.Datastructures
{
    class BinTree
    {
        public BinTree Left { get; set; }

        public BinTree Right { get; set; }
        
        public  BitSet Item { get; set; }

        public BinTree(BitSet item)
        {
            this.Item = item;
        }

        public void Tilt()
        {
            tilt();
        }

        private int tilt()
        {
            int leftTilt = this.Left?.tilt() ?? 0;
            int rightTilt = this.Right?.tilt() ?? 0;
            if (rightTilt < leftTilt)
            {
                BinTree temp = this.Left;
                this.Left = this.Right;
                this.Right = temp;
            }
            return Math.Max(leftTilt, rightTilt) + 1;
        }

        public static explicit operator BinTree(Tree tree)
        {
            BinTree binTree = new BinTree(tree.Root);
            Queue<BinTree> queue = new Queue<BinTree>();
            queue.Enqueue(binTree);

            while (queue.Count > 0)
            {
                BinTree node = queue.Dequeue();
                BitSet bitSet;
                if (tree.LeftChild.TryGetValue(node.Item, out bitSet))
                {
                    node.Left = new BinTree(bitSet);
                    queue.Enqueue(node.Left);
                }
                if (tree.RightChild.TryGetValue(node.Item, out bitSet))
                {
                    node.Right = new BinTree(bitSet);
                    queue.Enqueue(node.Right);
                }
            }
            return binTree;
        }
        
        public static explicit operator Tree(BinTree binTree)
        {
            Tree tree = new Tree();

            Queue<BinTree> queue = new Queue<BinTree>();
            queue.Enqueue(binTree);

            while (queue.Count > 0)
            {
                BinTree node = queue.Dequeue();
                tree.Insert(node.Item);
                if (node.Left != null)
                {
                    queue.Enqueue(node.Left);
                }
                if (node.Right != null)
                {
                    queue.Enqueue(node.Right);
                }
            }
            return tree;
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
                _recursionSimple(writer);
            }
            else
            {
                _recursion(writer);
            }
            writer.WriteLine("};");
            writer.WriteLine("\\end{tikzpicture}");
            writer.WriteLine("\\end{document}");
        }

        private void _recursion(TextWriter writer)
        {
            IList<string> str = new List<string>();
            int start = int.MaxValue;
            int prev = int.MaxValue;
            foreach (int item in this.Item)
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
            if (Left != null || Right != null)
            {
                writer.WriteLine("-> {");
                Left._recursion(writer);
                writer.WriteLine(",");
                Right._recursion(writer);
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine();
            }
        }

        private void _recursionSimple(TextWriter writer)
        {
            if (this.Item.Count == 1)
            {
                writer.WriteLine(this.Item.First());
            }
            else
            {
                writer.WriteLine("\"\"");
                writer.WriteLine("-> {");
                Left._recursionSimple(writer);
                writer.WriteLine(",");
                Right._recursionSimple(writer);
                writer.WriteLine("}");
            }
        }
    }
    
}
