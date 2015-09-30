using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class PerfectCode : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.PerfectCode; } }
        public PerfectCode(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = new BitSet(0, MaxSize, new int[] { 1 });
        }
    }
}