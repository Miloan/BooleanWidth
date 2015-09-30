using System.Collections.Generic;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules
{
    class PendantReductionRule : IReductionRule
    {
        public IEnumerable<IReductionRuleCommand> Find(Graph graph)
        {
            foreach (int vertex in graph.Vertices)
            {
                // TODO: Count is not O(1)
                if (graph.Degree(vertex) == 1 && graph.Vertices.Count > 1)
                {
                    BitSet connection = graph.OpenNeighborhood(vertex);
                    graph.RemoveVertex(vertex);
                    yield return new Command(vertex, connection);
                }
            }
        }

        public class Command : IReductionRuleCommand
        {
            public readonly int Vertex;
            public readonly BitSet Connection;

            public Command (int vertex, BitSet connection)
            {
                this.Vertex = vertex;
                this.Connection = connection;
            }

            public Tree Expand(Tree tree)
            {
                return ReductionRuleHelper.Expand(tree, this.Connection, this.Vertex);
            }
        }
    }
}
