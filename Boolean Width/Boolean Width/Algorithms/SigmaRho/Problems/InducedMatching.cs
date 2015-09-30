using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class InducedMatching : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.InducedMatching; } }
        public InducedMatching(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 1 });
            Rho = new BitSet(0, MaxSize);
            Rho = !Rho;
        }
    }
}