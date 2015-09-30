/*************************/
// Frank van Houten, 2014 - 2015
//
// Main algorithm for solving all Sigma-Rho problems using dynamic programming.
/*************************/

using System;
using System.Collections.Generic;
using BooleanWidth.Algorithms.SigmaRho.Problems;
using BooleanWidth.Datastructures;
using BooleanWidth.Datastructures.Decompositions;
using BooleanWidth.Datastructures.Representatives;

namespace BooleanWidth.Algorithms.SigmaRho
{
    public class GenericSolver
    {
        /*************************/
        // Basic attributes
        /*************************/

        // Tables contains table for a certain node
        // Table[A] returns us the full table that belongs to node A of the decomposition tree
        // Table[A][X, Y] = The optimal size of a set S, for which S≡X, and (S, Y) sigma,rho-dominates A
        private Dictionary<BitSet, Table> _tables;

        // The sigma-rho set that defines our problem instance
        private static SigmaRhoInstance _sigmaRhoInstance;

        // Optimum tells us wether we are looking for a maximum or minimum sigma-rho set
        private IOptimal _optimum;

        private RepresentativeTable _cuts;

        // Original graph that we calculate the sigma-rho set for
        private Graph _graph;

        // Boolean decomposition tree of the Graph
        private Tree _tree;

        /*************************/
        // Helper class for storing table entries for each individual node
        /*************************/

        // Each node of the decomposition tree has its own Table associated to it. 
        // Table[X, Y] = The optimal size of a set S, for which S≡X, and (S, Y) sigma,rho-dominates the node belonging to this table
        private class Table
        {
            // Data that we save in the table
            private Dictionary<Tuple<BitSet, BitSet>, int> _data;

            // Basic constructor
            public Table()
            {
                _data = new Dictionary<Tuple<BitSet, BitSet>, int>();
            }

            // Indexers used to access the values in this table in an array-like way
            public int this[BitSet inside, BitSet outside]
            {
                get { return _data[new Tuple<BitSet, BitSet>(inside, outside)]; }

                set { _data[new Tuple<BitSet, BitSet>(inside, outside)] = value; }
            }
        }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public GenericSolver(Decomposition decomposition)
        {
            // The graph and binary decomposition tree that we are working on
            _graph = decomposition.Graph;
            _tree = decomposition.Tree;
        }

        /*************************/
        // Algorithm for computing table values
        /*************************/

        // Returns the optimal value given a sigma,rho problem, and a binary decomposition tree of a graph
        public int Compute(IOptimal optimum, SigmaRhoInstance sigmaRhoInstance)
        {
            _sigmaRhoInstance = sigmaRhoInstance;
            _optimum = optimum;

            // Initialize the static parameters for our D-Neighborhoods class, i.e. the parameters that tell us how many neighbor we should check
            // in order to validate a sigma-rho set
            DNeighborhood.Initialize(sigmaRhoInstance);

            // Compute the full table of representatives
            // Cuts[A] gives us a list of all D-representatives at cut G(A, V\A))
            _cuts = new RepresentativeTable(_graph, _tree);

            // Initialize the empty table
            _tables = new Dictionary<BitSet, Table>();

            // Fill the entire table in a bottom up fashion
            FillTable(_graph.Vertices);

            // The final result will be found at T[V][empty, empty], since there is only one equivalence class at the root of the decomposition tree (namely the empty set)
            BitSet emptySet = new BitSet(0, _graph.Size);
            return _tables[_graph.Vertices][emptySet, emptySet];
        }

        public static int Counter = 0;

        // Base case for filling a table
        private void FillLeaf(BitSet leaf)
        {
            BitSet emptySet = new BitSet(0, _graph.Size);

            // Initialize all values by their default value, which is that a sigma-rho set does not exist
            foreach (BitSet representative in _cuts[_graph.Vertices - leaf])
            {
                _tables[leaf][emptySet, representative] = _optimum.PessimalValue;
                _tables[leaf][leaf, representative] = _optimum.PessimalValue;
            }

            // For each representative in G[V\{leaf}] we count the number of neighbors that the leaf has in this representative
            foreach (BitSet representative in _cuts[_graph.Vertices - leaf])
            {
                BitSet nleaf = _graph.Neighborhood(leaf);
                int count = (nleaf * representative).Count;

                // By definition it holds that if this number is contained in sigma, then the leaf plus representative is a valid sigma,rho-set of size 1
                if (_sigmaRhoInstance.Sigma.Contains(count))
                    _tables[leaf][leaf, representative] = 1;

                // The same reasoning applies to when this number is contained in rho, then the empty set plus this representative makes a valid sigma,rho-set of size 0
                if (_sigmaRhoInstance.Rho.Contains(count))
                    _tables[leaf][emptySet, representative] = 0;

                // If the empty set is a valid representative of the leaf vertex, then this may also be a valid set of size 1
                if (nleaf.Count == 0)
                    if (_sigmaRhoInstance.Sigma.Contains(count))
                        _tables[leaf][emptySet, representative] = 1;
            }

            Console.WriteLine("Processed {0} / {1} entries", ++Counter, _tree.Size);
        }

        // Recursively fills all tables for each node of the decomposition tree in a bottom up fashion
        // Node W is the current node we are working on
        private void FillTable(BitSet node)
        {
            // Initialize a new table of node W
            _tables[node] = new Table();

            // All vertices of the graph
            BitSet vg = _graph.Vertices;
            int n = _graph.Size;

            // The base case for leaf nodes is handled seperately
            if (node.Count == 1)
            {
                FillLeaf(node);
                return;
            }

            // If the node has a size >1 then there it has child nodes
            BitSet leftChild = _tree.LeftChild[node];
            BitSet rightChild = _tree.RightChild[node];

            // We work in a bottom-up fashion, so we first recurse on the two children of this node
            // We know that this node is not a leaf since we already passed the check for leaf-nodes
            // We also know that this node has two children, since having 1 child in a boolean decomposition means that the parent and child node are the same and thus redundant
            FillTable(leftChild);
            FillTable(rightChild);

            // Initially set all combinations of representatives to the worst value, meaning that no solution exists
            foreach (BitSet representative in _cuts[node])
                foreach (BitSet _representative in _cuts[vg - node])
                    _tables[node][representative, _representative] = _optimum.PessimalValue;

            // All representatives of the cut G[leftChild, V \ leftChild]
            foreach (BitSet representativeA in _cuts[leftChild])
            {
                // All representatives of the cut G[rightChild, V \ rightChild]
                foreach (BitSet representativeB in _cuts[rightChild])
                {
                    // All representatives of the cut G[V \ node, node]
                    foreach (BitSet _representative in _cuts[vg - node])
                    {
                        // Find the representative Ra_ of the class [Rb ∪ Rw_]≡Va_
                        DNeighborhood dna = new DNeighborhood(representativeB + _representative, leftChild, _graph);
                        BitSet representativeA_ = _cuts[vg - leftChild].GetRepresentative(dna);

                        // Find the representative Rb_ of the class [Ra ∪ Rw_]≡Vb_
                        DNeighborhood dnb = new DNeighborhood(representativeA + _representative, rightChild, _graph);
                        BitSet representativeB_ = _cuts[vg - rightChild].GetRepresentative(dnb);

                        // Find the representative Rw of the class [Ra ∪ Rb]≡Vw
                        DNeighborhood dnw = new DNeighborhood(representativeA + representativeB, vg - node, _graph);
                        BitSet representative = _cuts[node].GetRepresentative(dnw);

                        // Optimal size of this sigma,rho set in the left and right child
                        int leftValue = _tables[leftChild][representativeA, representativeA_];
                        int rightValue = _tables[rightChild][representativeB, representativeB_];

                        // Some hoops to avoid integer overflowing
                        int combination = leftValue == _optimum.PessimalValue || rightValue == _optimum.PessimalValue ?
                                                _optimum.PessimalValue : leftValue + rightValue;

                        // Fill the optimal value that we can find in the current entry
                        _tables[node][representative, _representative] = _optimum.Optimal(_tables[node][representative, _representative], combination);
                    }
                }
            }

        }

    }
}
