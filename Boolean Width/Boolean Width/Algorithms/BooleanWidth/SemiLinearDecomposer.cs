using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;

namespace BooleanWidth.Algorithms.BooleanWidth
{
    static class SemiLinearDecomposer
    {
        public static Decomposition Compute(Graph graph, Func<Graph, LinearDecomposition> decomposer, params IReductionRule[] rules)
        {
            Graph clone = graph.Clone();
            IReductionRuleCommand[] commands = GraphPreprocessor.ApplyRules(clone, rules).Reverse().ToArray();

            LinearDecomposition ld = decomposer(clone);
            Decomposition dec = Decomposition.FromLinearDecomposition(ld);
            Tree tree = commands.Aggregate(dec.Tree, (current, command) => command.Expand(current));
            return new Decomposition(graph, tree);
        }

        public static LinearDecomposition ConvertDecomposition(Decomposition decomposition)
        {
            BinTree tree = (BinTree)decomposition.Tree;
            tree.Tilt();
            BooleanChain chain = BooleanChain.DepthFirstSearch(tree.Left.Item, decomposition.MaxNeighborhoodSize, int.MaxValue, BooleanChain.FromGraph(decomposition.Graph, tree.Left.Item).ToArray());

            BinTree node = tree;
            while (node.Right != null)
            {
                node = node.Right;
                if (node.Left == null)
                {
                    chain = new BooleanChain(chain, node.Item.First());
                }
                else if (node.Left.Item.Count == 1)
                {
                    chain = new BooleanChain(chain, node.Left.Item.First());
                }
                else
                {
                    chain = BooleanChain.DepthFirstSearch(node.Left.Item, decomposition.MaxNeighborhoodSize, int.MaxValue, chain);
                }
            }
            return (LinearDecomposition)chain;
        }
    }
}
