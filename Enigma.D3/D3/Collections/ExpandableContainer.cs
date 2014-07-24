using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Enigma.D3.Memory;

namespace Enigma.D3.Collections
{
	public class ExpandableContainer : ExpandableContainer<MemoryObject>
	{
		public ExpandableContainer(MemoryBase memory, int address)
			: base(memory, address) { }
	}

	public class ExpandableContainer<T> : Container<T>, IEnumerable<T>
	{
		// 2.0.0.20874
		public new const int SizeOf = 0x168; // = 360

		public ExpandableContainer(MemoryBase memory, int address)
			: base(memory, address) { }

		public int x124 { get { return Field<int>(0x124); } }
		public int x128 { get { return Field<int>(0x128); } }
		public int _x12C { get { return Field<int>(0x12C); } }
		public BasicAllocator<T> x130_Allocator { get { return Field<BasicAllocator<T>>(0x130); } }
		public int _x14C { get { return Field<int>(0x14C); } }
		public int _x150 { get { return Field<int>(0x150); } }
		public int _x154 { get { return Field<int>(0x154); } }
		public MemoryManager.VTable x158_MemoryVTable { get { return Dereference<MemoryManager.VTable>(0x158); } }
		public int x15C_Limit { get { return Field<int>(0x15C); } }
		public int x160_MaxLimit_ { get { return Field<int>(0x160); } }
		public int x164_Bits { get { return Field<int>(0x164); } }

		public T this[short index]
		{
			get
			{
				var blockSize = 1 << x164_Bits;
				var blockNumber = index / blockSize;
				var blockOffset = index % blockSize;
				var blockBase = base.Memory.Read<int>(base.x120_Allocation + 4 * blockNumber);
				var itemPtr = blockBase + blockOffset * x104_ItemSize;
				var item = base.Memory.Read<T>(itemPtr);
				return item;
			}
		}

		public new IEnumerator<T> GetEnumerator()
		{
			short maxIndex = (short)base.x108_MaxIndex;
			if (maxIndex < 0)
				yield break;

			int itemSize = x104_ItemSize;
			int blockSize = 1 << x164_Bits;
			int blockCount = (maxIndex / blockSize) + 1;

			int[] blockPointers = base.Memory.Read<int>(x120_Allocation, blockCount);

			for (int i = 0; i <= maxIndex; i++)
			{
				int blockIndex = i / blockSize;
				int blockPointer = blockPointers[blockIndex];
				int blockOffset = itemSize * (i % blockSize);

				int itemAddress = blockPointer + blockOffset;

				yield return Memory.Read<T>(itemAddress);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public DumpInfo GetBufferDump(ref byte[] buffer)
		{
			var count = (short)x108_MaxIndex + 1;
			var blockCapacity = 1 << x164_Bits;
			var blockCount = x15C_Limit / blockCapacity;
			var itemSize = x104_ItemSize;
			var blockSize = blockCapacity * itemSize;

			System.Array.Resize(ref buffer, blockCount * blockSize);

			var dumpInfo = new DumpInfo(Memory, buffer, blockCount, itemSize, count);
			var blockPtrs = Memory.Read<int>(x120_Allocation, blockCount);
			for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
			{
				var blockAddress = blockPtrs[blockIndex];
				Memory.ReadBytes(blockAddress, buffer, blockIndex * blockSize, blockSize);
				dumpInfo.Blocks[blockIndex] = new DumpInfo.BlockInfo
				{
					BufferOffset = blockIndex * blockSize,
					Length = blockSize,
					Address = blockAddress
				};
			}
			return dumpInfo;
		}

		public class DumpInfo : IEnumerable<DumpInfo.Item>
		{
			public readonly BlockInfo[] Blocks;
			private readonly MemoryBase _memory;
			private readonly byte[] _buffer;
			public readonly int ItemSize;
			public readonly int ItemCount;

			public struct BlockInfo
			{
				public int Address;
				public int Length;
				public int BufferOffset;
			}

			public struct Item
			{
				public int Address;
				public int BufferOffset;
				public DumpInfo Dump;

				public T Create()
				{
					return (T)MemoryObject.UnsafeCreate(typeof(T), Dump._memory, Address, Dump._buffer, BufferOffset);
				}
			}

			public DumpInfo(MemoryBase memory, byte[] buffer, int blockCount, int itemSize, int itemCount)
			{
				Blocks = new BlockInfo[blockCount];
				_memory = memory;
				_buffer = buffer;
				ItemSize = itemSize;
				ItemCount = itemCount;
			}

			public IEnumerator<Item> GetEnumerator()
			{
				int index = 0;
				foreach (var block in Blocks)
				{
					int stop = Math.Min(ItemCount - index, block.Length / ItemSize);
					for (int i = 0; i < stop; i++)
					{
						yield return new Item { Dump = this, Address = block.Address + i * ItemSize, BufferOffset = i * ItemSize };
						index++;
					}
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public T[] GetFastDump<T>(ref byte[] buffer) where T : MemoryObject
		{
			var count = (short)x108_MaxIndex + 1;
			var array = new T[count];
			var blockCapacity = 1 << x164_Bits;
			var blockCount = x15C_Limit / blockCapacity;
			var itemSize = x104_ItemSize;
			var blockSize = blockCapacity * itemSize;

			// Resizes if required.
			if (buffer.Length != blockCount * blockSize)
			{
				System.Array.Resize(ref buffer, blockCount * blockSize);
			}

			var blockPtrs = Memory.Read<int>(x120_Allocation, blockCount);
			for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
			{
				var blockAddress = blockPtrs[blockIndex];
				Memory.ReadBytes(blockAddress, buffer, blockIndex * blockSize, blockSize);
				var iMax = count - blockIndex * blockCapacity;
				for (int i = 0; i < iMax; i++)
				{
					var obj = MemoryObject.Create<T>(Memory, blockAddress + itemSize * i);
					var memObj = obj as MemoryObject;
					memObj.SetSnapshot(buffer, blockIndex * blockSize + itemSize * i);
					array[blockIndex * blockCapacity + i] = obj;
				}
			}
			return array;
		}
	}
}