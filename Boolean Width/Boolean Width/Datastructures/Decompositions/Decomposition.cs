/*************************/
// Frank van Houten, 2014 - 2015
//
// Wrapper class for decompositions, which consist of a graph and a the actual decomposition tree.
/*************************/

using System;
using System.Linq;
using BooleanWidth.Algorithms.BooleanWidth;
using BooleanWidth.Algorithms.SigmaRho.Problems;
using BooleanWidth.Datastructures.Representatives;

namespace BooleanWidth.Datastructures.Decompositions
{
    public class Decomposition : IDecomposition
    {

        /*************************/
        // Basic attributes
        /*************************/
        // Ordering of the decomposition
        public readonly Tree Tree;

        // Graph belonging to this decompositon
        public Graph Graph { get; private set; }

        // Boolean-width belonging to this decomposition, by definition the log of the boolean-dimension
        public double BooleanWidth { get { return Math.Log(MaxNeighborhoodSize, 2); } }

        // Boolean-dimension of this decomposition, which we possibly have to calculate if not calculated before
        private long _maxNeighborhoodSize;
        public long MaxNeighborhoodSize
        {
            get
            {
                if (_maxNeighborhoodSize == 0)
                    _maxNeighborhoodSize = CalculateBooleanWidth();
                return _maxNeighborhoodSize;
            }
        }


        /*************************/
        // Constructor
        /*************************/
        public Decomposition(Graph graph, Tree tree)
        {
            Tree = tree;
            Graph = graph;
        }

        /*************************/
        // General methods
        /*************************/

        // Calculating the boolean dimension is done by computing all representatives
        private long CalculateBooleanWidth()
        {
            DNeighborhood.Initialize(new IndependentSet(2));
            RepresentativeTable cuts = new RepresentativeTable(Graph, Tree);
            return cuts.MaxDimension;
        }

        /*************************/
        // File IO
        /*************************/

        // Outputs the decomposition in BDC file format
        public void ToFile(string filename)
        {
            Tree.ToFile(filename);
        }
        public static Decomposition FromLinearDecomposition(LinearDecomposition ld)
        {
            Tree tree = new Tree();
            tree.Insert(ld.Graph.Vertices);
            BitSet parent = ld.Graph.Vertices;
            foreach (int item in ld.Sequence.Take(ld.Sequence.Count - 1))
            {
                tree.InsertWithParent(parent * item, parent);
                tree.InsertWithParent(parent - item, parent);
                parent = parent - item;
            }
            return new Decomposition(ld.Graph, tree)
            {
                // TODO: speed up using this
                //maxNeighborhoodSize = ld.MaxNeighborhoodSize
            };
        }
    }
}
