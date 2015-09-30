using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class TotalPerfectDominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.TotalPerfectDominatingSet; } }
        public TotalPerfectDominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 1 });
            Rho = new BitSet(0, MaxSize, new int[] { 1 });
        }
    }
}