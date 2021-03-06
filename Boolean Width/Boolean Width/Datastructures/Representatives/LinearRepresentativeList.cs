﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// The list of representatives is a bidirectional map, where we can access both the representative and the neighborhood it represents.
// This only saves representatives for sigma rho problems with d=1, i.e., regular representatives.
/*************************/

using System.Collections;

namespace BooleanWidth.Datastructures.Representatives
{
    public class LinearRepresentativeList : IEnumerable
    {
        /*************************/
        // Basic attributes
        /*************************/

        // The bidirectionalmap saves the elements, where in this case the elements are bitsets representing the one on one relation
        // between representatives and neighborhoods
        private BidirectionalMap<BitSet, BitSet> _map;

        // The count is simply the number of representatives in our map
        public long Count { get { return _map.Count; } }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public LinearRepresentativeList()
        {
            _map = new BidirectionalMap<BitSet, BitSet>();
        }

        /*************************/
        // Basic operations
        /*************************/

        // When adding a new entry we check if the neighborhood is already contained in the set
        // If true, then we might replace the previous representative that is not connected to this neighborhood if the new one is lexicographically smaller
        public void Update(BitSet representative, BitSet neighborhood)
        {
            if (_map.ContainsValue(neighborhood))
            {
                BitSet previousRep = _map.Reverse[neighborhood];

                if (representative.IsLexicographicallySmaller(previousRep))
                {
                    _map.Remove(previousRep, neighborhood);
                    _map.Add(representative, neighborhood);
                }
            }
            else
            {
                _map.Add(representative, neighborhood);
            }
        }

        // Returns the representative belonging to a given neighborhood
        public BitSet GetRepresentative(BitSet neighborhood)
        {
            return _map.Reverse[neighborhood];
        }

        // Returns the neighborhood belonging to a given representative
        public BitSet GetNeighborhood(BitSet representative)
        {
            return _map[representative];
        }

        public bool ContainsRepresentative(BitSet representative)
        {
            return _map.ContainsKey(representative);
        }

        public bool ContainsNeighborhood(BitSet neighborhood)
        {
            return _map.ContainsValue(neighborhood);
        }

        /*************************/
        //Enumerator
        /*************************/

        // Simple enumerator so that we can loop through all sets that are saved in the map
        public IEnumerator GetEnumerator()
        {
            return _map.GetEnumerator();
        }
    }
}
