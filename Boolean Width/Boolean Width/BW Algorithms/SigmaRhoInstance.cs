/*************************/
// Frank van Houten, 2014 - 2015
//
// Instances for multiple sigma,rho-set problems
/*************************/
using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    /*************************/
    // Abstract class for all sigma,rho-set problems
    /*************************/
    public abstract class SigmaRhoInstance
    {
        // The set Sigma, for which it should hold that forall v in X, |N(v) * X| in Sigma
        public BitSet Sigma { get; protected set; }

        // The set Sigma, for which it should hold that forall v in X, |N(v) * X| in Rho
        public BitSet Rho { get; protected set; }

        // Maximal size of a bitset which is used when defining new sigma,rho sets
        protected int MaxSize;

        // Basic constructor
        public SigmaRhoInstance(int n)
        {
            MaxSize = n;
        }

        // Gives us the type of the sets we are currently working with
        public abstract Abstract Type { get; }

        // Enumeration allows us to loop through all the different problems that we have available
        public enum Abstract { StrongStableSet, PerfectCode, TotallyNearlyPerfectSet, WeaklyPerfectDominatingSet, TotalPerfectDominatingSet,
        InducedMatching, DominatingInducedMatching, PerfectDominatingSet, IndependentSet, DominatingSet, IndependentDominatingSet, TotalDominatingSet
        }

        // We work with uninstantiated sigma rho sets so that we can pass them on whenever we loop through the collection of all sets
        public static SigmaRhoInstance Instantiate(Abstract type, int n)
        {
            switch (type)
            {
                case Abstract.StrongStableSet: return new StrongStableSet(n);
                case Abstract.PerfectCode: return new PerfectCode(n);
                case Abstract.TotallyNearlyPerfectSet: return new TotallyNearlyPerfectSet(n);
                case Abstract.WeaklyPerfectDominatingSet: return new WeaklyPerfectDominatingSet(n);
                case Abstract.TotalPerfectDominatingSet: return new TotalPerfectDominatingSet(n);
                case Abstract.InducedMatching: return new InducedMatching(n);
                case Abstract.DominatingInducedMatching: return new DominatingInducedMatching(n);
                case Abstract.PerfectDominatingSet: return new PerfectDominatingSet(n);
                case Abstract.IndependentSet: return new IndependentSet(n);
                case Abstract.DominatingSet: return new DominatingSet(n);
                case Abstract.IndependentDominatingSet: return new IndependentDominatingSet(n);
                case Abstract.TotalDominatingSet: return new TotalDominatingSet(n);
                default: throw new Exception("Inexhaustive operation enumeration.");
            }
        }
    }

    /*************************/
    // Actual sigma,rho-sets
    // Each sigma,rho class has to define both sets
    // In order to simulate all natural numbers we work with a bitset with all bits set to 1, i.e. 1111111111
    // This works because we will never exceed the number of vertices in the graph
    /*************************/
    public class StrongStableSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.StrongStableSet; } }
        public StrongStableSet(int n)
            : base(n)
        { 
            Sigma = new BitSet(0, MaxSize, new int [] { 0 });
            Rho = new BitSet(0, MaxSize, new int[] { 0, 1 });
        }
    }

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

    public class TotallyNearlyPerfectSet : SigmaRhoInstance
    {
        public override Abstract Type { get { return Abstract.TotallyNearlyPerfectSet; } }
        public TotallyNearlyPerfectSet(int n)
            : base(n)
        {
            Sigma = new BitSet(0, MaxSize, new int[] { 0, 1 });
            Rho = new BitSet(0, MaxSize, new int[] { 0, 1 });
        }
    }

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
