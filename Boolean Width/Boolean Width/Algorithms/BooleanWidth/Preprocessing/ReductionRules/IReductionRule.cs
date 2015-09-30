using System.Collections.Generic;
using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules
{
    interface IReductionRule
    {
        IEnumerable<IReductionRuleCommand> Find(Graph graph);
    }
}
