using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boolean_Width.Preprocessing.ReductionRules
{
    interface IReductionRule
    {
        IEnumerable<IReductionRuleCommand> Find(Graph graph);
    }

    interface IReductionRuleCommand
    {
        Tree Expand(Tree tree);
    }
}
