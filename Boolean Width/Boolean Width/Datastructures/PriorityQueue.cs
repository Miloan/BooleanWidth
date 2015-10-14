using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BooleanWidth.Datastructures
{
    /// <summary>
    /// The priorityqueue used in the pathfinding algorithms.
    /// It is based on Marks implementation of the heap from the class Datastructures
    /// </summary>
    /// <typeparam name="TValue">The abstract data to store in the queue</typeparam>
    class PriorityQueue<TKey, TValue>
    {
        private IComparer<TKey> _comparer; 
        public PriorityQueue(IComparer<TKey> comparer)
        {
            this._comparer = comparer;
            this.A = new List<PriorityObject>();
        }

        /// <summary>
        /// Enqueue a new item
        /// </summary>
        /// <param name="priority">The priority this item has</param>
        /// <param name="value">The item</param>
        public void Enqueue(TKey priority, TValue value)
        {
            Insert(new PriorityObject(priority, value));
        }

        /// <summary>
        /// Get the item with the highest priority (priority 1 is higher than priority 2)
        /// </summary>
        /// <param name="value">The value to return</param>
        /// <returns>Whether or not the dequeue has been successful</returns>
        public bool TryDequeue(out TValue value)
        {
            if (Count > 0)
            {
                value = Extract(0).Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Get the item with the highest priority (priority 1 is higher than priority 2)
        /// </summary>
        /// <returns>The value with the highest priority</returns>
        public TValue Dequeue()
        {
            return Extract(0).Value;
        }

        /// <summary>
        /// An object to represent the priority
        /// </summary>
        struct PriorityObject
        {
            public TKey Priority;
            public TValue Value;

            public PriorityObject(TKey priority, TValue value)
            {
                this.Priority = priority;
                this.Value = value;
            }
        }

        /// <summary>
        /// The array used in the heap
        /// </summary>
        private List<PriorityObject> A;

        /// <summary>
        /// Count the amount of elements in the queue
        /// </summary>
        public int Count
        {
            get
            {
                return A.Count;
            }
        }

        /// <summary>
        /// Get the priority of the element that will be dequeued next.
        /// </summary>
        public TKey NextPriority
        {
            get { return A[0].Priority; }
        }

        /// <summary>
        /// Make sure the heap will stay intact
        /// </summary>
        /// <param name="i">The index to look at</param>
        private void Heapify(int i)
        {
            int l = Left(i);
            int r = Right(i);
            int largest = i;
            if (l < Count && _comparer.Compare(A[largest].Priority, A[l].Priority) > 0)
            {
                largest = l;
            }
            if (r < Count && _comparer.Compare(A[largest].Priority, A[r].Priority) > 0)
            {
                largest = r;
            }

            if (largest != i)
            {
                PriorityObject temp = A[i];
                A[i] = A[largest];
                A[largest] = temp;
                Heapify(largest);
            }
        }

        /// <summary>
        /// Extract an element
        /// </summary>
        /// <param name="i">The index to extract from</param>
        /// <returns>The extracted object</returns>
        private PriorityObject Extract(int i = 0)
        {
            PriorityObject extracting = A[i];
            A[i] = A[Count - 1];
            ChangeKey(i, A[i]);
            A.RemoveAt(Count - 1);
            return extracting;
        }

        /// <summary>
        /// Insert an object to the queue
        /// </summary>
        /// <param name="key">The object to insert</param>
        private void Insert(PriorityObject key)
        {
            A.Add(key);
            ChangeKey(Count - 1, key);
        }

        /// <summary>
        /// Swap two values in the queue
        /// </summary>
        /// <param name="i">the index to swap on</param>
        /// <param name="key">the new value</param>
        private void ChangeKey(int i, PriorityObject key)
        {
            while (i > 0 && _comparer.Compare(A[Parent(i)].Priority, key.Priority) > 0)
            {
                A[i] = A[Parent(i)];
                i = Parent(i);
            }
            A[i] = key;
            Heapify(i);
        }

        /// <summary>
        /// Get the index of the left child
        /// </summary>
        /// <param name="i">The parent index</param>
        /// <returns>The index of the left child</returns>
        private static int Left(int i)
        {
            return 2 * i + 1;
        }

        /// <summary>
        /// Get the index of the right child
        /// </summary>
        /// <param name="i">The parent index</param>
        /// <returns>The index of the right child</returns>
        private static int Right(int i)
        {
            return 2 * i + 2;
        }

        /// <summary>
        /// Get the index of the parent
        /// </summary>
        /// <param name="i">The child index</param>
        /// <returns>The index of the parent</returns>
        private static int Parent(int i)
        {
            return (i - 1) / 2;
        }
    }
}
