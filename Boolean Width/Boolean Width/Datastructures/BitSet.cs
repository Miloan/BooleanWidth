/*************************/
// Frank van Houten, 2014 - 2015
//
// A BitSet is a datastructure used to represent a set of integers in a given range.
// Set operations such as union, join, difference and inverse can all be done in O(1) time using a bitset.
/*************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Block = System.UInt64;    // Alternate definition

namespace Boolean_Width
{

	public class BitSet : IEnumerable
    {
        /*************************/
        // Basic attributes
        /*************************/

		// The lower and uppderbound of the range of integers that we will save
        private readonly int LowerBound;
        private readonly int UpperBound;

		// The actual data is stored inside this Data
		// Each index of the Data, called a Block, can store 64 true/false values for distinct integers that we want to save
		// We can view this collection as a bitarray
		private Block[] Data;

        // This bool tells us if we already have initialized the bitmasks for all integers in our range. We check if all bitsets are initialized when we add an element, since that should be the first time
        // that we need to use an identifier.
        // This can be replaced by a pre-processing step in order to avoid doing an unneeded if-check every single time. The upside to not doing pre-processing is that we an user of the bitset class does
        // not have to think about initializing all bitsets.
        private static bool Initialized = false;

		// Each block has a blocksize of 64. This value should be equal to the size of bits of the undelrying data representative (called Block)
		protected static readonly int BlockSize = 64; // DO NOT CHANGE, number of bits in each block, which is the number of bits in an ulong currently

        // Check if there is any bit set to 1 in the data collection
		public bool IsEmpty 
		{ 
			get 
			{
				for (int i = 0; i < Data.Length; i++)
					if (Data[i] != 0)
						return false;
				return true;
			}
		}

		// Returns the number of elements that are actually contained in this BitSet.
        public int Count { get { return count(); } }

        // Count the number of bits set to 1 using the Brian Kernigan method
        private int count()
        {
            int count = 0;
            for (int i = 0; i < Data.Length; i++)
                for (Block n = Data[i]; n != 0; count++)
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

            LowerBound = lowerBound;
            UpperBound = upperBound;

			// For each 64 consecutive integers we need a single block
			int blocks = (((upperBound - 1) - lowerBound) / BlockSize) + 1; 

			Data = new Block[blocks];
        }

		public BitSet(int lowerBound, int upperBound, params int[] args)
			: this(lowerBound, upperBound)
		{
			AddRange(args);
		}

        /*************************/
        // Simple querying and manipulation
        /*************************/

        // Adds a single integer by projecting an integer to a unique binary value that will be added to the proper ulong in the correct block.
        public void Add(int i)
        {
            if (!CheckBounds(i))
                throw new Exception("Element is out of bounds for the BitSet");

            // Initialize the bitmasks for all integers in the range
            // When we add an item it should be the first time that we actually need to initialize all identifiers
            if (!Initialized)
                Identifiers.Initialize();

            // Don't readd elements that are already contained in this set
            if (Contains(i))
                return;

            // Obtain the location and bitmask of the given integer
            Block id = Blockify(i);
			int location = i / BlockSize;

            // Inserting an item simply means adding the bitmask value to the saves 64-bit block
			Data[location] += id;
        }

        // Add a range of integers to the bitset by repeated insertion
        public void AddRange(BitSet s)
        {
            foreach (int i in s)
                Add(i);
        }

        // Add a range of integers to the bitset by repeated insertion
        public void AddRange(IEnumerable<int> elements)
        {
            foreach (int i in elements)
                Add(i);
        }

        // Removes a single integer value by subtracting the unique binary value from the proper block.
		// Returns true if the removal is successful and false otherwise.
        public void Remove(int i)
        {
            if (!CheckBounds(i))
                throw new Exception("Element is out of bounds for the bitset");

            if (IsEmpty)
                throw new Exception("Cannot remove an element from an empty bitset.");

            if (!Contains(i))
                throw new Exception("Cannot remove element from the set since it is not contained in the bitset");

			// Internal id of the integer, this is the unique bit it is saved under
            Block id = Blockify(i);

			// Block that this integer is saved under
            int location = i / BlockSize;

			// Update the value in the block accordingly, resulting in a removal operation
			Data[location] -= id;
        }

        // Removes a range of integers from the bitset by repeated removal
        public void RemoveRange(IEnumerable<int> elements)
        {
            foreach (int i in elements)
                Remove(i);
        }

        // Returns true if a certain integer value is contained in our set. 
        public bool Contains(int i)
        {
            if (!CheckBounds(i))
                return false;

            Block id = Blockify(i);
            return (id & Data[i / BlockSize]) == id;
        }

        // Returns the first element from the bitset
		public int First()
		{
			if (IsEmpty)
				throw new Exception("Cannot pick an element from an empty bitset.");

            // Simply find the first value that is set to true in this set
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] == 0)
                    continue;

                Block n = Data[i];
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

            int position = Data.Length - 1;
            while (Data[position] == 0)
                position--;

            Block n = Data[position];
            Block block = n;
            while (n != 0)
            {
                block = n;
                n &= (n - 1);
            }

            return BitPosition(block, position);
        }

        // Returns an exact copy of this bitset
        public BitSet Copy()
		{
			Func<Block, Block> func = x => x;
			return Map (func);
		}

        // Empties this bitset
        public void Clear()
        {
            int blocks = ((UpperBound - LowerBound) / BlockSize) + 1;
            Data = new Block[blocks];
        }

        /*************************/
        // Mapping
        /*************************/

        // Applies a function Block -> Block to every block in our data, for instance !x
        // In essense this means that we apply a certain function to every bit in the block
        private BitSet Map(Func<Block, Block> func)
        {
            BitSet result = new BitSet(LowerBound, UpperBound);

            for (int i = 0; i < Data.Length; i++)
                result.Data[i] = func(Data[i]);

            return result;
        }

        // Applies a function Block, Block -> Block to every block in our data, for instance x & y
        // In essense this means that we apply a certain function to every bit in the block
        private BitSet Map(Func<Block, Block, Block> func, BitSet s)
        {
            if (!CheckBounds(s))
                throw new ArgumentException("Cannot perform an operation on two bitsets with different bounds or blocks.");

            BitSet result = new BitSet(LowerBound, UpperBound);

            for (int i = 0; i < Data.Length; i++)
                result.Data[i] = func(Data[i], s.Data[i]);

            return result;
        }

        /*************************/
        // Basic set operations
        /*************************/

        // Returns a new bitset that is the union of the given element and this bitset
        public BitSet Union(int i) 
		{
            BitSet result = Copy();
			result.Add (i);
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
			return Contains(i) ? new BitSet(LowerBound, UpperBound) { i } : new BitSet(LowerBound, UpperBound);
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
            int range = UpperBound - LowerBound;
            // We only have to make this change if there are not exactly a power of 64 number of elements in our bitset.
            // If range % 64 then the inverse function already computed the correct values
            if (range % 64 != 0)
            {
                Block u =  (Block)(1UL << range) - 1;
                inverse.Data[Data.Length - 1] = u & inverse.Data[Data.Length - 1];
            }

            return inverse;
        }

        // Returns a new bitset that is the difference of this bitset and the given element
        public BitSet Difference(int i) 
		{
            BitSet result = Copy();
            if (Contains(i))
			    result.Remove (i);
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
            return (i >= LowerBound && i < UpperBound);
        }

        // Checks if an operation can be performed on this bitset and the given bitset
        private bool CheckBounds(BitSet s)
        {
		    return (s.LowerBound == LowerBound 
					&& s.UpperBound == UpperBound
					&& s.Data.Length == Data.Length);
        }

        /*************************/
        // Enumerator
        /*************************/

        // Returns an IEnumerator to enumerate through the set, using the Brian Kernigan method to locate actual set bits.
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Data.Length; i++)           // O(|X| / 64)
            {
                Block n = Data[i];
                while (n != 0)
                {
                    Block remainder = n & (n - 1);          // O(1)
                    Block block = n - remainder;            // O(1)
                    n = remainder;                          // O(1)
                    yield return BitPosition(block, i);     // O(1) per element
                }
            }
        }

        /*************************/
        // Identifiers
        /*************************/

        // Given an integer i that is valid for this bitset, Blockify(i) returns the 2^id, where id is the position where i is saved
        // Example: Given an integer 5 for the bitset (0,10), it returns 2^5 = 32 (which is 00100000)
        private Block Blockify(int i)
		{
            return Identifiers.BitPowers[(i - LowerBound) % BlockSize];
		}

        // BitPosition returns, given a block for an integer and the position of this block, the actual integer that this block represents
        //Example: Given a block of 32 (which is 00100000) for a bitset of (0,10), it returns 2log(32) = 5 
        private int BitPosition(Block block, int blockNumber)
        {
            if (!Identifiers.BitPositions.ContainsKey(block))
                throw new Exception(string.Format("{0} is not a valid power of 2, cannot look up the bit position of this value", block));

            return Identifiers.BitPositions[block] + (blockNumber * 64) + LowerBound;
        }

        // Each integer is projected to a unique power of 2 which represents a binary value.
        // We can use this binary value to easily add and subtract values of sets, therefore giving us easy manipulation
        // techniques for union, intersection and difference calculations.

        static class Identifiers
        {
            public static Block[] BitPowers;
            public static Dictionary<Block, int> BitPositions;

            public static void Initialize()
            {
                BitPowers = new Block[BitSet.BlockSize];
                BitPositions = new Dictionary<Block, int>();

                for (int i = 0; i < BitSet.BlockSize; i++)
                {
                    Block id = (Block)1L << i;
                    BitPowers[i] = id;
                    BitPositions[id] = i;
                }

                Initialized = true;
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

            for (int i = 0; i < Data.Length; i++)
                if (Data[i] != other.Data[i])
                    return false;

            return true;
        }

        // Hashcode implementation of combining all values of an array of longs
        // Taken from the book 'Effective Java' by Joshua Bloch
        public override int GetHashCode()
        {
            int result = 23;

            foreach (Block v in Data)
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
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] == other.Data[i])
                    continue;

                // Example: 0111011 vs 0101111
                // The xor gives us 0010100
                Block xor = Data[i] ^ other.Data[i];
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
