using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boolean_Width.Preprocessing.ReductionRules
{
    class TwinReductionRule : IReductionRule
    {
        public IEnumerable<IReductionRuleCommand> Find(Graph graph)
        {
            foreach (int v in graph.Vertices)
            {
                foreach (int u in graph.Vertices)
                {
                    if (v == u)
                    {
                        break;
                    }
                    // TODO: Count is not O(1)
                    if (graph.Vertices.Count > 1 && ((graph.OpenNeighborhood(v) - u).Equals(graph.OpenNeighborhood(u) - v)))
                    {
                        graph.RemoveVertex(v);
                        yield return new Command(v, u);
                        break;
                    }
                }
            }
        }

        public class Command : IReductionRuleCommand
        {
            public readonly int Vertex;
            public readonly int Twin;

            public Command(int vertex, int twin)
            {
                this.Vertex = vertex;
                this.Twin = twin;
            }

            public Tree Expand(Tree tree)
            {
                return ReductionRuleHelper.Expand(tree, tree.Root * this.Twin, this.Vertex);
            }
        }
    }
}
