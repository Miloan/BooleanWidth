using System.Collections.Generic;
using BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing
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
