/*************************/
// Frank van Houten, 2014 - 2015
//
// Implementation of Algorithm 14, 'MaxIS' by Sadia Sharmin in her PhD-thesis 'Practical Aspects of the Graph Parameter Boolean-Width'.
// Algorithm for counting the number of independent sets in a graph, given a linear decomposition of the graph
/*************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Boolean_Width
{
    public static class LinearMaximumIS
    {
        public static int Compute(LinearDecomposition decomposition)
        {
            Graph graph = decomposition.Graph;
            List<int> sequence = decomposition.Sequence;

            // Left is the set of vertices that have been processed so far.
            BitSet left = new BitSet(0, graph.Size);

            // Right is the set of vertices that we have yet to process, initially set to all vertices in the graph.
            BitSet right = graph.Vertices;

            // The leftneighborset contains ISN elements, that save the size of each independent set in Left, and the corresponding neighborhood in Right that belongs to this IS.
            LookupTable table = new LookupTable();

            // Insert the initial neighborhood and IS of size 0 and an empty neighborhood.
            table.Update(new ISN(new BitSet(0, graph.Size), 0));

            // maxIS saves the largest independent set that we have encountered so far.
            int maxIS = 0;

            // Loop over all vertices in the sequence; we have to process all of them.
            for (int i = 0; i < sequence.Count; i++)
            {
                // Take the next vertex in the sequence.
                int v = sequence[i]; 

                //Save all updated element in a new neighborset, so that we do not disturb any looping over elements.
                LookupTable newTable = new LookupTable();

                foreach (ISN iset in table)
                {
                    // If a neighborhood Right contains the currently selected element, then we have to remove it from the neighborhood.
                    // The corresponding IS in Left is still valid, but it simply belongs to a smaller neighborhood in Right (hence the removal of v).
                    if (iset.Elements.Contains(v))
                    {
                        iset.Elements -= v;
                        newTable.Update(iset);
                    }
                    // If the neighborhood does not contain v, then there are two cases that have to be handled.
                    else
                    {
                        // Case a: If v is not an element of the largest IS, then the previous IS is still valid.
                        // We have no risk of v being contained in the neighborhood of Right, because if that would be the case we would not be in the else-part of the if-statement.
                        // Thus, we simply add an unmodified copy of j to the new list of neighborhoodsets.
                        newTable.Update(iset);

                        // Case b: If v is an element of the largest IS, then we should include a new entry for this newly created IS.
                        // The size of this IS will increase by one (adding v will cause this).
                        // The neighborhood of this IS is the old neighborhood, plus any vertices in Right that are in the neighborhood of v. Vertex v causes the addition of these vertices.
                        // The largest saved IS might change because of this addition of a new erlement.
                        ISN newIset = new ISN(iset.Elements, iset.Size);
                        newIset.Size++;
                        newIset.Elements = newIset.Elements + (graph.OpenNeighborhood(v) * right);
                        maxIS = Math.Max(maxIS, newIset.Size);
                        newTable.Update(newIset);
                    }
                }

                // Safely update all sets that we are working with
                left += v;
                right -= v;
                table = newTable;

            }
            // The largest IS that we have encountered is the one we will return
            return maxIS;
        }

        // A neighborhood set consists of a hashset of unique identifiers to ISN elements.
        // The unique string representation is needed so that we can map elements with the same neighborhood to the same independent set.
        class LookupTable : IEnumerable
        {
            // The hash set of all saved ISNs.
            private Dictionary<BitSet, ISN> Data;

            public LookupTable()
            {
                Data = new Dictionary<BitSet, ISN>();
            }

            // Update will insert or update values of the hashset, depending on if an element is already contained in the set or not.
            public void Update(ISN iset)
            {
                // Firstly check if we already have an entry for this ISN. If we don't; simply add the ISN to the set using its hash representation.
                if (Data.ContainsKey(iset.Elements))
                {
                    // If we do, then update the size if the new size is larger than the currently saved size.
                    // This corresponds to each neighborhood only saving the size of the largest IS in Left, since that is the only IS we are interested in.
                    if (iset.Size > Data[iset.Elements].Size)
                        Data[iset.Elements] = iset;
                }
                else
                {
                    Data[iset.Elements] = iset;
                }
            }

            // Allows us to loop over all elements in a foreach-statement. We're not interested in the keys of the dictionary.
            IEnumerator IEnumerable.GetEnumerator()
            {
                return Data.Values.GetEnumerator();
            }

            // Simple string representation of all elements in this set. 
            public override string ToString()
            {
                if (Data.Count == 0)
                    return "{ }";

                StringBuilder sb = new StringBuilder();
                sb.Append("{ ");
                foreach (ISN j in this)
                {
                    sb.Append(j);
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append(" }");
                return sb.ToString();
            }
        }

        // Independent Set Neighborhoods
        // An ISN consists of the Size of the Independent Set that it represents in Left, and the elements of the neighborhood in Right, ie. Neighborhood(IS) * Right.
        // This way, for each IS in Left we have the current size and we have the corresponding neighborhood that is yet to be processed.
        class ISN
        {
            public int Size;
            public BitSet Elements;

            public ISN(BitSet elements, int size) 
            {
                Size = size;
                Elements = elements;
            }

            public override string ToString()
            {
                return string.Format("({0}, {1})", Elements, Size);
            }
        }
    }
}
