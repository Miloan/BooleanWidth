/*************************/
// Frank van Houten, 2014 - 2015
//
// The list of representatives is a bidirectional map, where we can access both the representative and the neighborhood it represents.
/*************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace Boolean_Width
{
    public class RepresentativeList : IEnumerable
    {
        /*************************/
        // Basic attributes
        /*************************/

        // The bidirectionalmap saves the elements, where in this case the elements are bitsets representing the one on one relation
        // between representatives and neighborhoods
        private BidirectionalMap<BitSet, dNeighborhood> Map;

        // The count is simply the number of representatives in our map
        public long Count { get { return Map.Count; } }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public RepresentativeList()
        {
            Map = new BidirectionalMap<BitSet, dNeighborhood>();
        }

        /*************************/
        // Basic operations
        /*************************/

        // When adding a new entry we check if the neighborhood is already contained in the set
        // If true, then we might replace the previous representative that is not connected to this neighborhood if the new one is lexicographically smaller
        public void Update(BitSet representative, dNeighborhood neighborhood)
        {
            Map.Add(representative, neighborhood);
        }

        // Returns the representative belonging to a given neighborhood
        public BitSet GetRepresentative(dNeighborhood neighborhood)
        {
            return Map.Reverse[neighborhood];
        }

        // Returns the neighborhood belonging to a given representative
        public dNeighborhood GetNeighborhood(BitSet representative)
        {
            return Map[representative];
        }

        public bool ContainsRepresentative(BitSet representative)
        {
            return Map.ContainsKey(representative);
        }

        public bool ContainsNeighborhood(dNeighborhood neighborhood)
        {
            return Map.ContainsValue(neighborhood);
        }

        /*************************/
        //Enumerator
        /*************************/

        // Simple enumerator so that we can loop through all sets that are saved in the map
        public IEnumerator GetEnumerator()
        {
            return Map.GetEnumerator();
        }
    }
}
