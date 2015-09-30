using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class PerfectDominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.PerfectDominatingSet; } }
        public PerfectDominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize);
            Sigma = !Sigma;
            Rho = new BitSet(0, MaxSize, new int[] { 1 });
        }
    }
}