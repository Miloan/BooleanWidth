using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boolean_Width;
using Boolean_Width.Preprocessing.ReductionRules;

namespace Boolean_Width.Preprocessing
{
    static class GraphPreprocessor
    {
        public static IEnumerable<IReductionRuleCommand> ApplyRules(Graph graph, IEnumerable<IReductionRule> rules)
        {
            bool found;
            do
            {
                found = false;

                foreach (IReductionRule rule in rules)
                {
                    foreach (IReductionRuleCommand command in rule.Find(graph))
                    {
                        found = true;
                        yield return command;
                    }
                }
            }
            while (found);
        }
    }
}
