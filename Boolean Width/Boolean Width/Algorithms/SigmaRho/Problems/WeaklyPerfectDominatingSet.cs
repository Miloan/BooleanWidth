using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class WeaklyPerfectDominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.WeaklyPerfectDominatingSet; } }
        public WeaklyPerfectDominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0, 1 });
            Rho = new BitSet(0, MaxSize, new int[] { 1 });
        }
    }
}