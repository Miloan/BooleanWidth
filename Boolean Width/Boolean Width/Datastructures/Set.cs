/*************************/
// Frank van Houten, 2014 - 2015
//
// A set of objects. A set can only contain unique objects, and offers methods to search, add and remove objects quickly.
/*************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Boolean_Width
{

    public class Set<T> : IEnumerable<T>
    {
        /*************************/
        // Basic attributes
        /*************************/

        // Data contained in the set.
        private List<T> Data;

        // The position of element T tells us on which index in the list the object is actually saved.
        private Dictionary<T, int> Position;

        // Boolean dictionary to indicate wether or not element T is contained in the set.
        private Dictionary<T, bool> Contained;

        // Number of elements contained in the set.
        public int Count { get { return Data.Count; } }

        // Returns true if there are no elements contained in the set.
        public bool IsEmpty { get { return Count == 0; } }

        /*************************/
        // Construction
        /*************************/

        // Creates a new empty set.
        public Set()
        {
            Data = new List<T>();
            Position = new Dictionary<T, int>();
            Contained = new Dictionary<T, bool>();
        }

        
        // Creates a new set in which the input elements will be stored.
        public Set(params T[] elements)
            : this()
        {
            AddRange(elements);
        }

        
        // Creates a new set in which we store all values contained in data.
        public Set(IEnumerable<T> data)
            : this()
        {
            AddRange(data);
        }

        /*************************/
        // Basic Set Operations
        // Set operations always result in a newly created set that is returned to the user, instead of modifying the set that the operation is called on.
        /*************************/

        
        // Returns the union of this set and a single element.
        public Set<T> Union(T element)
        {
            Set<T> union = new Set<T>(this);
            union.Add(element);
            return union;
        }


        // Returns the union of the called set and the set given as a parameter.
        public Set<T> Union(Set<T> data)
        {
            Set<T> union = new Set<T>(this);
            union.AddRange(data);
            return union;
        }


        // Returns the intersection of the called set and the set given as a parameter.
        public Set<T> Intersection(T element)
        {
            return Contains(element) ? new Set<T>(element) : new Set<T>();
        }


        // Returns the intersection of the called set and the set given as a parameter.
        public Set<T> Intersection(Set<T> data)
        {
            Set<T> intersection = new Set<T>();
            Set<T> smallest = Count <= data.Count ? this : data;
            Set<T> other = Count > data.Count ? this : data;

            foreach (T element in smallest)
                if (other.Contains(element))
                    intersection.Add(element);

            return intersection;
        }


        // Returns the difference of this set and a single element.
        public Set<T> Difference(T element)
        {
            Set<T> difference = new Set<T>(this);
            if (Contains(element))
                difference.Remove(element);
            return difference;
        }


        // Returns the difference of the called set and the set given as a parameter.
        public Set<T> Difference(Set<T> data)
        {
            Set<T> difference = new Set<T>();

            foreach (T element in Data)
                if (!(data.Contains(element)))
                    difference.Add(element);

            return difference;
        }

		
		// Perform a certain function on each element of the set.
        public void Map(Func<T, T> func)
		{
			foreach (T element in Data)
				func (element);
		}

        /*************************/
        // Querying and manipulation
        /*************************/

        
        //  Returns an element of the set without removing it.
        public T Pick()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Set is empty.");

            return Data[0];
        }

        
        // Returns and removes an element of the set.
        public T Pop()
        {
            T element = Pick();
            Remove(element);
            return element;
        }

        
        // Add a specified element into the set.
        public void Add(T element)
        {
            // Adds should always be unique
            if (Contains(element))
                return;

			Position[element] = Count;
            Contained[element] = true;
            Data.Add(element);
        }

        
        // Adds a range of elements to the set.
        public void Add(params T[] elements)
        {
            AddRange(elements);
        }

        
        // Adds a range of elements to the set.
        public void AddRange(IEnumerable<T> data)
        {
            foreach (T element in data)
                Add(element);
        }

        
        // Removes the specified element from the set.
        public void Remove(T element)
        {
            if (!Contains(element))
                throw new InvalidOperationException("Cannot remove element from the set since it is not contained in the set");

			int last = Count - 1;
            Position[Data[last]] = Position[element];         // tell the last vertex' position that it has moved to the position of identifier
            Data[Position[element]] = Data[last];       // the spot of the to be removed vertex v will be replaced by the last vertex of the list
            Contained[element] = false;
            Data.RemoveAt(last);                              //O(1) removing
        }

        
        // Removes a range of elements from the set.
        public void RemoveRange(IEnumerable<T> data)
        {
            foreach (T element in data)
                Remove(element);
        }

        
        // Indicates wether or not a specified element is contained in this set.>
        public bool Contains(T element)
        {
            bool b = false;
            Contained.TryGetValue(element, out b);
            return b;
        }

        
        // Removes all elements from the set.
        public void Clear()
        {
            Contained.Clear();
            Data.Clear();
            Position.Clear();
        }

        
        // Copies the current data into a new set.
        public Set<T> Copy()
        {
            return new Set<T>(this);
        }

        /*************************/
        // Conversion
        /*************************/

        
        // Creates a list of all the elements contained in the set.
        public List<T> ToList()
        {
            return new List<T>(Data);
        }

        /*************************/
        // Enumerator
        /*************************/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        // Returns an IEnumerator to enumerate through the set.
        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        /*************************/
        // Printing
        /*************************/

        public override string ToString()
        {
            if (IsEmpty)
                return "{ }";

            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            foreach (T i in this)
            {
                sb.Append(i);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(" }");
            return sb.ToString();
        }
    }
}
