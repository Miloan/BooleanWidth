using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class DominatingInducedMatching : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.DominatingInducedMatching; } }
        public DominatingInducedMatching(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 1 });
            Rho = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = !Rho;
        }
    }
}