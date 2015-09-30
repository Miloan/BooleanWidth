using System.Collections.Generic;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules
{
    static class ReductionRuleHelper
    {
        public static BitSet BreadthFirstSearch (Tree tree)
        {

            Queue<BitSet> queue = new Queue<BitSet>();
            queue.Enqueue(tree.Root);
            BitSet prev = null;
            while (queue.Count > 0)
            {
                BitSet node = queue.Dequeue();
                BitSet left = null, right = null;
                tree.LeftChild.TryGetValue(node, out left);
                tree.RightChild.TryGetValue(node, out right);
                if (left == null && right == null)
                {
                    prev = node;
                }
                else
                {
                    if (left != null)
                    {
                        queue.Enqueue(left);
                    }
                    if (right != null)
                    {
                        queue.Enqueue(right);
                    }
                }
            }

            return prev;
        }

        public static Tree Expand(Tree tree, BitSet parent, int node)
        {
            Tree newTree = new Tree();

            Queue<BitSet> queue = new Queue<BitSet>();
            queue.Enqueue(tree.Root);
            
            while (queue.Count > 0)
            {
                BitSet set = queue.Dequeue();
                BitSet child;
                if (tree.LeftChild.TryGetValue(set, out child))
                {
                    queue.Enqueue(child);
                }
                if (tree.RightChild.TryGetValue(set, out child))
                {
                    queue.Enqueue(child);
                }
                if (parent.IsSubsetOf(set))
                {
                    set += node;
                }

                newTree.Insert(set);
            }

            newTree.Insert(parent);
            newTree.Insert(newTree.Root * node);

            return newTree;
        }

        //public static void Expand (Tree tree, BitSet parent, int node)
        //{
        //    BitSet x = parent.Copy();
        //    AddToNode(tree, parent, node);
        //    tree.InsertWithParent(x, parent);
        //    tree.InsertWithParent(parent * node, parent);

            
        //    while (tree.Parent.TryGetValue(parent, out parent))
        //    {
        //        if (!parent.Contains(node))
        //        {
        //            AddToNode(tree, parent, node);
        //        }
        //    }
        //    Console.WriteLine("Expanded");
        //}

        //public static void AddToNode (Tree tree, BitSet set, int node)
        //{
        //    Console.WriteLine("Add " + node + " to " + set.Count);
        //    BitSet update = set.Copy();
        //    update.Add(node);

        //    updateDictionary(tree.Parent, set, update);
        //    updateDictionary(tree.LeftChild, set, update);
        //    updateDictionary(tree.RightChild, set, update);
        //}
        //private static void updateDictionary(Dictionary<BitSet, BitSet> dict, BitSet oldSet, BitSet newSet)
        //{
        //    BitSet temp;
        //    if (dict.TryGetValue(oldSet, out temp))
        //    {
        //        dict.Remove(oldSet);
        //        dict[newSet] = temp;
        //    }
        //}
    }
}
