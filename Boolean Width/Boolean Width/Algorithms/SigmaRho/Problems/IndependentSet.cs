using BooleanWidth.Datastructures;

namespace BooleanWidth.Algorithms.SigmaRho.Problems
{
    public class IndependentSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.IndependentSet; } }
        public IndependentSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0 });
            Rho = new BitSet(0, MaxSize);
            Rho = !Rho;
        }
    }
}