/*************************/
// Frank van Houten, 2014 - 2015
//
// dNeighborhoods are a generalization of regular neighborhoods
// The dNeighborhood of a set X of A is defined as a multiset of elements of V\A, where a vertex v of V\A occurs exactly min(d, N(v) * A) times in the multiset 
/*************************/

using System;
using System.Collections.Generic;
using BooleanWidth.Algorithms.BooleanWidth;
using BooleanWidth.Algorithms.SigmaRho.Problems;

namespace BooleanWidth.Datastructures
{
    public class DNeighborhood
    {
        // The vector is the set V\A, which is the same for all dNeighborhoods of a cut (A, V\A)
        public BitSet Vector;

        // Occurences is the multiset that saves for each element of V\A how often it occurs in the multiset
        private Dictionary<int, int> _occurrences;

        // The dValue of a neighborhood is a problem specific constant that is needed for determining the number of occurences of a vertex in the dNeighborhood
        private static int dValue { get { return DValue.Value; } }

        // Static class for determining the dValue given a specific problem.
        // The dValue can either be hard set or can be set by initializing the dNeighborhood by using a specific problem instance
        static class DValue
        {
            public static int Value { get; private set; }

            public static void Initialize(SigmaRhoInstance sigmaRhoInstance)
            {
                Value = Math.Max(DetermineDValue(sigmaRhoInstance.Sigma), DetermineDValue(sigmaRhoInstance.Rho));
            }

            // Determines the dValue given set
            // For a set x, d is defined as follows:
            // d(N) = 0 
            // d(x) = 1 + min(max v in x, max v not in x)
            private static int DetermineDValue(BitSet set)
            {
                BitSet _set = !set;

                if (set.IsEmpty || _set.IsEmpty)
                    return 0;
            
                return 1 + Math.Min(set.Last(), _set.Last());
            }
        }

        // Basic constructor for a dNeighborhood
        public DNeighborhood(BitSet vector)
        {
            _occurrences = new Dictionary<int, int>();
            Vector = vector;

            foreach (int v in Vector)
                _occurrences[v] = 0;

        }

        // Initializes the dValue for a dNeighborhood, given a problem instance
        public static void Initialize(SigmaRhoInstance sigmaRhoInstance)
        {
            DValue.Initialize(sigmaRhoInstance);
        }

        // Given a vertex w, we can update the dNeighborhood of a set X to reflect the dNeighborhood of the set X + w in O(n) time
        public DNeighborhood CopyAndUpdate(Graph graph, int w)
        {
            // initialize an empty dNeighborhood in O(|Vector|) time
            DNeighborhood nx = new DNeighborhood(Vector);

            // Copy all values in O(|Vector|) time
            foreach (int v in Vector)
                nx._occurrences[v] = _occurrences[v];

            // Foreach vertex in N(w) * Vector they will appear one extra time in the multiset
            // This operation take O(n) time because of the bitset operation
            foreach (int v in graph.OpenNeighborhood(w) * Vector)
                nx._occurrences[v] = Math.Min(dValue, nx._occurrences[v] + 1);

            return nx;
        }

        public DNeighborhood CopyAndUpdateVector(BitSet vector, bool increment)
        {
            // initialize an empty dNeighborhood in O(|Vector|) time
            DNeighborhood nx = new DNeighborhood(vector);

            BitSet iterateOver = increment ? Vector : vector;
            foreach (int v in iterateOver)
                nx._occurrences[v] = _occurrences[v];

            return nx;
        }

        // Builds a neighborhood for a set X from the ground on up, without relying on what has been saved previously in O(n^2) time
        public DNeighborhood(BitSet x, BitSet vector, Graph graph)
        {
            Vector = vector;
            _occurrences = new Dictionary<int, int>();

            // Loops in O(|Vector|) time over all vertices in the vector
            foreach (int v in Vector)
            {
                // Bitset operation of O(n) time
                BitSet nv = graph.OpenNeighborhood(v) * x;
                _occurrences[v] = Math.Min(nv.Count, dValue);
            }
        }

        /*************************/
        // Comparing
        /*************************/

        // Check if two dNeighborhoods are equal
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is DNeighborhood))
                return false;

            DNeighborhood other = obj as DNeighborhood;

            if (!other.Vector.Equals(Vector))
                return false;

            foreach (KeyValuePair<int, int> pair in _occurrences)
            {
                if (!other._occurrences.ContainsKey(pair.Key))
                    return false;
                if (other._occurrences[pair.Key] != _occurrences[pair.Key])
                    return false;
            }

            return true;
        }

        // Generates a hash code for a collection, in this case a dictionary
        // Based on the approach described in the book 'Effective Java' by Joshua Bloch
        public override int GetHashCode()
        {
            int result = Vector.GetHashCode();
            foreach (KeyValuePair<int, int> pair in _occurrences)
                result = (result * 37) + pair.Key * 31 + pair.Value * 701;
            return result;
        }
    }
}
