using System.Collections.Generic;
using System.Diagnostics;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules
{
    class IsletReductionRule : IReductionRule
    {
        public IEnumerable<IReductionRuleCommand> Find(Graph graph)
        {
            foreach (int vertex in graph.Vertices)
            {
                if (graph.Degree(vertex) == 0)
                {
                    graph.RemoveVertex(vertex);
                    yield return new Command(vertex);
                }
            }
        }

        public class Command : IReductionRuleCommand
        {
            public readonly int Vertex;

            public Command (int vertex)
            {
                this.Vertex = vertex;
            }

            public Tree Expand(Tree tree)
            {
                BitSet parent = tree.Root;
                while (parent.Count > 1 && tree.RightChild.TryGetValue(parent, out parent))
                { }
                return ReductionRuleHelper.Expand(tree, parent, this.Vertex);
            }
        }
    }
}
