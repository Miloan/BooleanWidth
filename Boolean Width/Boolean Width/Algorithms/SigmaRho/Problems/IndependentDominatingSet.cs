using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class IndependentDominatingSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.IndependentDominatingSet; } }
        public IndependentDominatingSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = !Rho;
        }
    }
}