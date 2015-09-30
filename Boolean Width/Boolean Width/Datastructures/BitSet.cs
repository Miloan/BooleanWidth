﻿/*************************/
// Frank van Houten, 2014 - 2015
//
// A BitSet is a datastructure used to represent a set of integers in a given range.
// Set operations such as union, join, difference and inverse can all be done in O(1) time using a bitset.
/*************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Block = System.UInt64;    // Alternate definition

namespace BooleanWidth.Datastructures
{

	public class BitSet : IEnumerable, IEnumerable<int>
    {
        /*************************/
        // Basic attributes
        /*************************/

		// The lower and uppderbound of the range of integers that we will save
        private readonly int _lowerBound;
        private readonly int _upperBound;

		// The actual data is stored inside this Data
		// Each index of the Data, called a Block, can store 64 true/false values for distinct integers that we want to save
		// We can view this collection as a bitarray
		private readonly Block[] _data;

		// Each block has a blocksize of 64. This value should be equal to the size of bits of the undelrying data representative (called Block)
		protected static readonly int BlockSize = 64; // DO NOT CHANGE, number of bits in each block, which is the number of bits in an ulong currently

        // Check if there is any bit set to 1 in the data collection
		public bool IsEmpty 
		{ 
			get
			{ return _data.All(t => t == 0); }
		}

        private int? _count;
        
		// Returns the number of elements that are actually contained in this BitSet.
        public int Count
        {
            get
            {
                if (!_count.HasValue)
                {
                    _count = count();
                }
                else if (_count != count())
                {
                    throw new Exception("The counting is not right.");
                }
                return _count.Value;
            }
        }

        // Count the number of bits set to 1 using the Brian Kernigan method
        private int count()
        {
            int count = 0;
            for (int i = 0; i < _data.Length; i++)
                for (Block n = _data[i]; n != 0; count++)
                    n &= n - 1;
            return count;
        }

        /*************************/
        // Constructors
        /*************************/

		// Initializes a new instance of the BitSet class.
		// Lower bound (included) of the range of integers that will be saved.
		// Upper bound (excluded) of the range of integers that will be saved.
        public BitSet(int lowerBound, int upperBound)
        {
            if (lowerBound > upperBound)
                throw new Exception("Lowerbound cannot be strictly higher than upperbound");

            _lowerBound = lowerBound;
            _upperBound = upperBound;

			// For each 64 consecutive integers we need a single block
			int blocks = (((upperBound - 1) - lowerBound) / BlockSize) + 1; 

			_data = new Block[blocks];

            _count = 0;
        }

        public BitSet(int lowerBound, int upperBound, IEnumerable<int> args)
            : this(lowerBound, upperBound)
        {
            foreach (int i in args)
                _add(i);
        }

        public BitSet(int lowerBound, int upperBound, params int[] args)
            : this(lowerBound, upperBound, (IEnumerable<int>)args)
        { }

        /*************************/
        // Simple querying and manipulation
        /*************************/

        // Adds a single integer by projecting an integer to a unique binary value that will be added to the proper ulong in the correct block.
        private void _add(int i)
        {
            if (!CheckBounds(i))
                throw new Exception("Element is out of bounds for the BitSet");

            // Don't readd elements that are already contained in this set
            if (Contains(i))
                return;

            if (_count.HasValue)
                _count++;

            // Obtain the location and bitmask of the given integer
            Block id = Blockify(i);
			int location = i / BlockSize;

            // Inserting an item simply means adding the bitmask value to the saves 64-bit block
			_data[location] += id;
        }

        // Removes a single integer value by subtracting the unique binary value from the proper block.
		// Returns true if the removal is successful and false otherwise.
        private void _remove(int i)
        {
            if (!CheckBounds(i))
                throw new Exception("Element is out of bounds for the bitset");

            if (IsEmpty)
                throw new Exception("Cannot remove an element from an empty bitset.");

            if (!Contains(i))
                throw new Exception("Cannot remove element from the set since it is not contained in the bitset");

            if (_count.HasValue)
                _count--;

            // Internal id of the integer, this is the unique bit it is saved under
            Block id = Blockify(i);

			// Block that this integer is saved under
            int location = i / BlockSize;

			// Update the value in the block accordingly, resulting in a removal operation
			_data[location] -= id;
        }

        // Returns true if a certain integer value is contained in our set. 
        public bool Contains(int i)
        {
            if (!CheckBounds(i))
                return false;

            Block id = Blockify(i);
            return (id & _data[i / BlockSize]) == id;
        }

        // Returns the first element from the bitset
		public int First()
		{
			if (IsEmpty)
				throw new Exception("Cannot pick an element from an empty bitset.");

            // Simply find the first value that is set to true in this set
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] == 0)
                    continue;

                Block n = _data[i];
                Block rest = n & (n - 1);
                Block block = n - rest;
                return BitPosition(block, i);
            }

			return -1;
		}

        // Returns the last element in the bitset in O(log n) time
        public int Last()
        {
            if (IsEmpty)
                throw new Exception("Cannot pick an element from an empty bitset.");

            int position = _data.Length - 1;
            while (_data[position] == 0)
                position--;

            Block n = _data[position];
            Block block = n;
            while (n != 0)
            {
                block = n;
                n &= (n - 1);
            }

            return BitPosition(block, position);
        }

        // Returns an exact copy of this bitset
        private BitSet _copy()
		{
			Func<Block, Block> func = x => x;
            BitSet set = Map(func);
            if (_count.HasValue)
            {
                set._count = _count;
            }
            return set;
		}

        /*************************/
        // Mapping
        /*************************/

        // Applies a function Block -> Block to every block in our data, for instance !x
        // In essense this means that we apply a certain function to every bit in the block
        private BitSet Map(Func<Block, Block> func)
        {
            BitSet result = new BitSet(_lowerBound, _upperBound)
            {
                _count = null
            };

            for (int i = 0; i < _data.Length; i++)
                result._data[i] = func(_data[i]);

            return result;
        }

        // Applies a function Block, Block -> Block to every block in our data, for instance x & y
        // In essense this means that we apply a certain function to every bit in the block
        private BitSet Map(Func<Block, Block, Block> func, BitSet s)
        {
            if (!CheckBounds(s))
                throw new ArgumentException("Cannot perform an operation on two bitsets with different bounds or blocks.");

            BitSet result = new BitSet(_lowerBound, _upperBound)
            {
                _count = null
            };

            for (int i = 0; i < _data.Length; i++)
                result._data[i] = func(_data[i], s._data[i]);

            return result;
        }

        /*************************/
        // Basic set operations
        /*************************/

        // Returns a new bitset that is the union of the given element and this bitset
        public BitSet Union(int i) 
		{
            BitSet result = _copy();
			result._add (i);
			return result;
		}

        // Returns a new bitset that is the union of the given bitset and this bitset
        public BitSet Union(BitSet s)
        {
			Func<Block, Block, Block> func = (x, y) => x | y;
			return Map (func, s); 
        }

        // Returns a new bitset that is the intersection of the given element and this bitset
        public BitSet Intersection(int i) 
		{
			return Contains(i) ? new BitSet(_lowerBound, _upperBound, new int[] { i }) : new BitSet(_lowerBound, _upperBound);
		}

        // Returns a new bitset that is the intersection of the given bitset and this bitset
        public BitSet Intersection(BitSet s)
        {
			Func<Block, Block, Block> func = (x, y) => x & y;
			return Map (func, s);
        }

        // Returns a new bitset that is the inverse of this bitset, i.e. 0001 becomes 1110
        public BitSet Inverse()
        {
            // First we obtain the inverse by mapping an inverse function over all blocks of data
			Func<Block, Block> func = x => ~ x;
            BitSet inverse = Map(func);

            // However, the very last block should not be fully reversed because of the upperbound
            // We don't want values that were out of bounds to be suddenly included in the range

            // The lowerbound will be fixed automatically; we only save our actual elements in the range 0...n, where n = upperbound - lowerbound
            // The upperbound will be fixed as follows:
            // Imagine the saved bitset is 000101 with an upperbound of 3 elements, and we already computed the inverse; 111010 using our Map function.
            // We now compute bitset u = 000111, and intersect this with the inverse resulting in 000010, which is exactly the inverse we were looking for.
            int range = _upperBound - _lowerBound;
            // We only have to make this change if there are not exactly a power of 64 number of elements in our bitset.
            // If range % 64 then the inverse function already computed the correct values
            if (range % 64 != 0)
            {
                Block u =  (Block)(1UL << range) - 1;
                inverse._data[_data.Length - 1] = u & inverse._data[_data.Length - 1];
            }

            if (_count.HasValue)
            {
                inverse._count = range - _count;
            }

            return inverse;
        }

        // Returns a new bitset that is the difference of this bitset and the given element
        public BitSet Difference(int i) 
		{
            BitSet result = _copy();
            if (Contains(i))
			    result._remove (i);
			return result;
		}

        // Returns a new bitset that is the difference of this bitset and the given bitset
        public BitSet Difference(BitSet s)
        {
			Func<Block, Block, Block> func = (x, y) => x - (y & x);
			return Map (func, s);
        }

        // Returns true if this bitset is a subset of the bitset given as a parameter
        public bool IsSubsetOf(BitSet s)
        {
            return (this + s).Equals(s);
        }

        /*************************/
        // Operator definitions
        /*************************/

        // Overloaded operator definition
        public static BitSet operator !(BitSet s)
        {
            return s.Inverse();
        }

        public static BitSet operator +(BitSet s, BitSet t)
        {
            return s.Union(t);
        }

        public static BitSet operator *(BitSet s, BitSet t)
        {
            return s.Intersection(t);
        }

        public static BitSet operator -(BitSet s, BitSet t)
        {
            return s.Difference(t);
        }

        public static BitSet operator +(BitSet s, int i)
        {
            return s.Union(i);
        }

        public static BitSet operator *(BitSet s, int i)
        {
            return s.Intersection(i);
        }

        public static BitSet operator -(BitSet s, int i)
        {
            return s.Difference(i);
        }

        /*************************/
        // Helper methods
        /*************************/

        // Checks if a certain integer can be contained in this bitset
        private bool CheckBounds(int i)
        {
            return (i >= _lowerBound && i < _upperBound);
        }

        // Checks if an operation can be performed on this bitset and the given bitset
        private bool CheckBounds(BitSet s)
        {
		    return (s._lowerBound == _lowerBound 
					&& s._upperBound == _upperBound
					&& s._data.Length == _data.Length);
        }

        /*************************/
        // Enumerator
        /*************************/

        // Returns an IEnumerator to enumerate through the set, using the Brian Kernigan method to locate actual set bits.
        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < _data.Length; i++)           // O(|X| / 64)
            {
                Block n = _data[i];
                while (n != 0)
                {
                    Block remainder = n & (n - 1);          // O(1)
                    Block block = n - remainder;            // O(1)
                    n = remainder;                          // O(1)
                    yield return BitPosition(block, i);     // O(1) per element
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /*************************/
        // Identifiers
        /*************************/

        // Given an integer i that is valid for this bitset, Blockify(i) returns the 2^id, where id is the position where i is saved
        // Example: Given an integer 5 for the bitset (0,10), it returns 2^5 = 32 (which is 00100000)
        private Block Blockify(int i)
		{
            return Identifiers.BitPowers[(i - _lowerBound) % BlockSize];
		}

        // BitPosition returns, given a block for an integer and the position of this block, the actual integer that this block represents
        //Example: Given a block of 32 (which is 00100000) for a bitset of (0,10), it returns 2log(32) = 5 
        private int BitPosition(Block block, int blockNumber)
        {
            if (!Identifiers.BitPositions.ContainsKey(block))
                throw new Exception(string.Format("{0} is not a valid power of 2, cannot look up the bit position of this value", block));

            return Identifiers.BitPositions[block] + (blockNumber * 64) + _lowerBound;
        }

        // Each integer is projected to a unique power of 2 which represents a binary value.
        // We can use this binary value to easily add and subtract values of sets, therefore giving us easy manipulation
        // techniques for union, intersection and difference calculations.

        static class Identifiers
        {
            public static Block[] BitPowers;
            public static Dictionary<Block, int> BitPositions;

            static Identifiers ()
            {
                BitPowers = new Block[BitSet.BlockSize];
                BitPositions = new Dictionary<Block, int>();

                for (int i = 0; i < BitSet.BlockSize; i++)
                {
                    Block id = (Block)1L << i;
                    BitPowers[i] = id;
                    BitPositions[id] = i;
                }
            }
        }

        /*************************/
        // Comparing
        /*************************/

        // Compares this bitset to the bitset given as a paramter
        // Returns true if they are equal
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is BitSet))
                return false;

            BitSet other = obj as BitSet;

            if (!CheckBounds(other))
                return false;

            for (int i = 0; i < _data.Length; i++)
                if (_data[i] != other._data[i])
                    return false;

            return true;
        }

        // Hashcode implementation of combining all values of an array of longs
        // Taken from the book 'Effective Java' by Joshua Bloch
        public override int GetHashCode()
        {
            int result = 23;

            foreach (Block v in _data)
                result = (result * 37) + (int)(v ^ (v >> 32));

            return result;
        }

        // Returns true if this set is lexicographically smaller than the input bitset
        public bool IsLexicographicallySmaller(BitSet other)
        {
            // The set with the least number of elements is automatically smaller than the other
            if (Count < other.Count)
                return true;
            
            if (Count > other.Count)
                return false;

            // If the number of elements is equal then we check which set has a bit set to 1 that the other set doesnt,
            // where this bit is the first bit where this happens
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] == other._data[i])
                    continue;

                // Example: 0111011 vs 0101111
                // The xor gives us 0010100
                Block xor = _data[i] ^ other._data[i];
                // rest gives us 0010100 & 0010011 = 001000
                Block rest = xor & (xor - 1);
                // bit gives us 0010100 - 001000 = 0000100, which is exactly the first bit set to 1 in one set and 0 in the other
                Block bit = xor - rest;
                // Obtain the actual integer that this bit represents
                int id = BitPosition(bit, i);
                // If this set contains this bit, then it is lexicographically smaller
                return this.Contains(id);
            }

            return false;
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
            foreach (int i in this)
            {
                sb.Append(i);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(" }");
            return sb.ToString();
        }

        /*************************/
        // Conversion
        /*************************/

        public List<int> ToList()
        {
            List<int> result = new List<int>();
            foreach (int i in this)
                result.Add(i);
            return result;
        }
    }
}
