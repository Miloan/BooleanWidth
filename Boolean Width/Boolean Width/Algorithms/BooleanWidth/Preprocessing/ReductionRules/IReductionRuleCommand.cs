using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.BooleanWidth.Preprocessing.ReductionRules
{
    interface IReductionRuleCommand
    {
        Tree Expand(Tree tree);
    }
}