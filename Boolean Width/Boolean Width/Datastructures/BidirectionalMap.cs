/*************************/
// Frank van Houten, 2014 - 2015
//
// A Bidirectional map is a collection of items where there is a one to one correspondance between objects in set 1 and set 2.
// Given an element of one of the two sets we can easily find the connected element in the other set.
/*************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace BooleanWidth.Datastructures
{
    public class BidirectionalMap<T1, T2> : IEnumerable
    {
        /*************************/
        // Basic attributes
        /*************************/

        // All elements in a forward fashion
        private Dictionary<T1, T2> _forward;

        // All elements in a backward fashion
        private Dictionary<T2, T1> _backward;

        // We use this indexer so that everything can be accessed in reverse order
        public Indexer Reverse { get; private set; }

        // Number of elements in the map
        public int Count { get { return _forward.Count; } }

        /*************************/
        // Constructor
        /*************************/

        // Basic constructor
        public BidirectionalMap() 
        {
            _forward = new Dictionary<T1, T2>();
            _backward = new Dictionary<T2, T1>();

            Reverse = new Indexer(this);
        }

        /*************************/
        // Indexer
        /*************************/

        // Default get and set operations
        public T2 this[T1 index]
        {
            get { return _forward[index]; }
            set
            {
                Add(index, value);
            }
        }

        /*************************/
        // Basic operations
        /*************************/

        // Returns true if the given key is present in the map
        public bool ContainsKey(T1 key)
        {
            return _forward.ContainsKey(key);
        }

        // Returns true if the given value is present in the map
        public bool ContainsValue(T2 value)
        {
            return _backward.ContainsKey(value);
        }

        // Adds a keyvalue-pair to the map, same as Map[key] = value
        public void Add(T1 key, T2 value)
        {
            if (_forward.ContainsKey(key) ^ _backward.ContainsKey(value))
                throw new Exception("Key/value is already contained; insertion is only allowed if key and value are not yet contained, or both are contained");

            _forward[key] = value;
            _backward[value] = key;
        }

        // Removes a keyvalue-pair from the map
        public void Remove(T1 key, T2 value)
        {
            if (!(ContainsKey(key) && ContainsValue(value)))
                throw new Exception("Key or value is not contained in bidirectional map");

            _forward.Remove(key);
            _backward.Remove(value);
        }

        // Clears the map
        public void Clear()
        {
            _forward.Clear();
            _backward.Clear();
        }

        /*************************/
        // Enumerator
        /*************************/

        public IEnumerator GetEnumerator()
        {
            return _forward.Keys.GetEnumerator();
        }

        /*************************/
        // Indexer class for reverse operations
        /*************************/
        public class Indexer : IEnumerable
        {
            // All actual data is saved in the parent class
            private BidirectionalMap<T1, T2> _parent;

            public int Count { get { return _parent._backward.Count; } }
            
            // Basic constructor
            public Indexer(BidirectionalMap<T1, T2> parent)
            {
                _parent = parent;
            }

            // Reverse indexer
            public T1 this[T2 index]
            {
                get { return _parent._backward[index]; }
                set
                {
                    Add(index, value);
                }
            }

            // Reverse adding of data, same as Map.Reverse[value] = key
            public void Add(T2 value, T1 key)
            {
                if (_parent._forward.ContainsKey(key) ^ _parent._backward.ContainsKey(value))
                    throw new Exception("Key/value is already contained; insertion is only allowed if key and value are not yet contained, or both are contained");

                _parent._forward[key] = value;
                _parent._backward[value] = key;
            }

            // Basic enumerator
            public IEnumerator GetEnumerator()
            {
                return _parent._backward.Keys.GetEnumerator();
            }
        }

    }
}
