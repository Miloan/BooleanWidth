/*************************/
// Frank van Houten, 2014 - 2015
//
// The list of representatives is a bidirectional map, where we can access both the representative and the neighborhood it represents.
// This only saves representatives for sigma rho problems with d=1, i.e., regular representatives.
/*************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Boolean_Width
{
    public class LinearRepresentativeList : IEnumerable
    {
        /*************************/
        // Basic attributes
        /*************************/

        // The bidirectionalmap saves the elements, where in this case the elements are bitsets representing the one on one relation
        // between representatives and neighborhoods
        private BidirectionalMap<BitSet, BitSet> Map;

        // The count is simply the number of representatives in our map
        public long Count { get { return Map.Count; } }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public LinearRepresentativeList()
        {
            Map = new BidirectionalMap<BitSet, BitSet>();
        }

        /*************************/
        // Basic operations
        /*************************/

        // When adding a new entry we check if the neighborhood is already contained in the set
        // If true, then we might replace the previous representative that is not connected to this neighborhood if the new one is lexicographically smaller
        public void Update(BitSet representative, BitSet neighborhood)
        {
            if (Map.ContainsValue(neighborhood))
            {
                BitSet previousRep = Map.Reverse[neighborhood];

                if (representative.IsLexicographicallySmaller(previousRep))
                {
                    Map.Remove(previousRep, neighborhood);
                    Map.Add(representative.Copy(), neighborhood.Copy());
                }
            }
            else
            {
                Map.Add(representative.Copy(), neighborhood.Copy());
            }
        }

        // Returns the representative belonging to a given neighborhood
        public BitSet GetRepresentative(BitSet neighborhood)
        {
            return Map.Reverse[neighborhood];
        }

        // Returns the neighborhood belonging to a given representative
        public BitSet GetNeighborhood(BitSet representative)
        {
            return Map[representative];
        }

        public bool ContainsRepresentative(BitSet representative)
        {
            return Map.ContainsKey(representative);
        }

        public bool ContainsNeighborhood(BitSet neighborhood)
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
