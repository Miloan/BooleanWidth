using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class TotalDominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.TotalDominatingSet; } }
        public TotalDominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0 });
            Sigma = !Sigma;
            Rho = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = !Rho;
        }
    }
}