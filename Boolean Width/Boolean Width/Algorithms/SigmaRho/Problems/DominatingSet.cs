using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class DominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.DominatingSet; } }
        public DominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize);
            Sigma = !Sigma;
            Rho = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = !Rho;
        }
    }
}