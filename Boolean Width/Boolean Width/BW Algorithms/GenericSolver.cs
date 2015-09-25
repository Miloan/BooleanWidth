/*************************/
// Frank van Houten, 2014 - 2015
//
// Main algorithm for solving all Sigma-Rho problems using dynamic programming.
/*************************/
using System;
using System.Collections.Generic;

namespace Boolean_Width
{
    public class GenericSolver
    {
        /*************************/
        // Basic attributes
        /*************************/

        // Tables contains table for a certain node
        // Table[A] returns us the full table that belongs to node A of the decomposition tree
        // Table[A][X, Y] = The optimal size of a set S, for which S≡X, and (S, Y) sigma,rho-dominates A
        private Dictionary<BitSet, Table> Tables;

        // The sigma-rho set that defines our problem instance
        private static SigmaRhoInstance SigmaRhoInstance;

        // Optimum tells us wether we are looking for a maximum or minimum sigma-rho set
        private IOptimal Optimum;

        private RepresentativeTable Cuts;

        // Original graph that we calculate the sigma-rho set for
        private Graph Graph;

        // Boolean decomposition tree of the Graph
        private Tree Tree;

        /*************************/
        // Helper class for storing table entries for each individual node
        /*************************/

        // Each node of the decomposition tree has its own Table associated to it. 
        // Table[X, Y] = The optimal size of a set S, for which S≡X, and (S, Y) sigma,rho-dominates the node belonging to this table
        private class Table
        {
            // Data that we save in the table
            private Dictionary<Tuple<BitSet, BitSet>, int> Data;

            // Basic constructor
            public Table()
            {
                Data = new Dictionary<Tuple<BitSet, BitSet>, int>();
            }

            // Indexers used to access the values in this table in an array-like way
            public int this[BitSet inside, BitSet outside]
            {
                get { return Data[new Tuple<BitSet, BitSet>(inside, outside)]; }

                set { Data[new Tuple<BitSet, BitSet>(inside, outside)] = value; }
            }
        }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public GenericSolver(Decomposition decomposition)
        {
            // The graph and binary decomposition tree that we are working on
            Graph = decomposition.Graph;
            Tree = decomposition.Tree;
        }

        /*************************/
        // Algorithm for computing table values
        /*************************/

        // Returns the optimal value given a sigma,rho problem, and a binary decomposition tree of a graph
        public int Compute(IOptimal optimum, SigmaRhoInstance sigmaRhoInstance)
        {
            SigmaRhoInstance = sigmaRhoInstance;
            Optimum = optimum;

            // Initialize the static parameters for our D-Neighborhoods class, i.e. the parameters that tell us how many neighbor we should check
            // in order to validate a sigma-rho set
            dNeighborhood.Initialize(sigmaRhoInstance);

            // Compute the full table of representatives
            // Cuts[A] gives us a list of all D-representatives at cut G(A, V\A))
            Cuts = new RepresentativeTable(Graph, Tree);

            // Initialize the empty table
            Tables = new Dictionary<BitSet, Table>();

            // Fill the entire table in a bottom up fashion
            FillTable(Graph.Vertices);

            // The final result will be found at T[V][empty, empty], since there is only one equivalence class at the root of the decomposition tree (namely the empty set)
            BitSet emptySet = new BitSet(0, Graph.Size);
            return Tables[Graph.Vertices][emptySet, emptySet];
        }

        public static int counter = 0;

        // Base case for filling a table
        private void FillLeaf(BitSet leaf)
        {
            BitSet emptySet = new BitSet(0, Graph.Size);

            // Initialize all values by their default value, which is that a sigma-rho set does not exist
            foreach (BitSet _representative in Cuts[Graph.Vertices - leaf])
            {
                Tables[leaf][emptySet, _representative] = Optimum.PessimalValue;
                Tables[leaf][leaf, _representative] = Optimum.PessimalValue;
            }

            // For each representative in G[V\{leaf}] we count the number of neighbors that the leaf has in this representative
            foreach (BitSet _representative in Cuts[Graph.Vertices - leaf])
            {
                BitSet nleaf = Graph.Neighborhood(leaf);
                int count = (nleaf * _representative).Count;

                // By definition it holds that if this number is contained in sigma, then the leaf plus representative is a valid sigma,rho-set of size 1
                if (SigmaRhoInstance.Sigma.Contains(count))
                    Tables[leaf][leaf, _representative] = 1;

                // The same reasoning applies to when this number is contained in rho, then the empty set plus this representative makes a valid sigma,rho-set of size 0
                if (SigmaRhoInstance.Rho.Contains(count))
                    Tables[leaf][emptySet, _representative] = 0;

                // If the empty set is a valid representative of the leaf vertex, then this may also be a valid set of size 1
                if (nleaf.Count == 0)
                    if (SigmaRhoInstance.Sigma.Contains(count))
                        Tables[leaf][emptySet, _representative] = 1;
            }

            Console.WriteLine("Processed {0} / {1} entries", ++counter, Tree.Size);
        }

        // Recursively fills all tables for each node of the decomposition tree in a bottom up fashion
        // Node W is the current node we are working on
        private void FillTable(BitSet node)
        {
            // Initialize a new table of node W
            Tables[node] = new Table();

            // All vertices of the graph
            BitSet VG = Graph.Vertices;
            int n = Graph.Size;

            // The base case for leaf nodes is handled seperately
            if (node.Count == 1)
            {
                FillLeaf(node);
                return;
            }

            // If the node has a size >1 then there it has child nodes
            BitSet leftChild = Tree.LeftChild[node];
            BitSet rightChild = Tree.RightChild[node];

            // We work in a bottom-up fashion, so we first recurse on the two children of this node
            // We know that this node is not a leaf since we already passed the check for leaf-nodes
            // We also know that this node has two children, since having 1 child in a boolean decomposition means that the parent and child node are the same and thus redundant
            FillTable(leftChild);
            FillTable(rightChild);

            // Initially set all combinations of representatives to the worst value, meaning that no solution exists
            foreach (BitSet representative in Cuts[node])
                foreach (BitSet _representative in Cuts[VG - node])
                    Tables[node][representative, _representative] = Optimum.PessimalValue;

            // All representatives of the cut G[leftChild, V \ leftChild]
            foreach (BitSet representative_a in Cuts[leftChild])
            {
                // All representatives of the cut G[rightChild, V \ rightChild]
                foreach (BitSet representative_b in Cuts[rightChild])
                {
                    // All representatives of the cut G[V \ node, node]
                    foreach (BitSet _representative in Cuts[VG - node])
                    {
                        // Find the representative Ra_ of the class [Rb ∪ Rw_]≡Va_
                        dNeighborhood dna = new dNeighborhood(representative_b + _representative, leftChild, Graph);
                        BitSet _representative_a = Cuts[VG - leftChild].GetRepresentative(dna);

                        // Find the representative Rb_ of the class [Ra ∪ Rw_]≡Vb_
                        dNeighborhood dnb = new dNeighborhood(representative_a + _representative, rightChild, Graph);
                        BitSet _representative_b = Cuts[VG - rightChild].GetRepresentative(dnb);

                        // Find the representative Rw of the class [Ra ∪ Rb]≡Vw
                        dNeighborhood dnw = new dNeighborhood(representative_a + representative_b, VG - node, Graph);
                        BitSet representative = Cuts[node].GetRepresentative(dnw);

                        // Optimal size of this sigma,rho set in the left and right child
                        int leftValue = Tables[leftChild][representative_a, _representative_a];
                        int rightValue = Tables[rightChild][representative_b, _representative_b];

                        // Some hoops to avoid integer overflowing
                        int combination = leftValue == Optimum.PessimalValue || rightValue == Optimum.PessimalValue ?
                                                Optimum.PessimalValue : leftValue + rightValue;

                        // Fill the optimal value that we can find in the current entry
                        Tables[node][representative, _representative] = Optimum.Optimal(Tables[node][representative, _representative], combination);
                    }
                }
            }

        }

    }
}
