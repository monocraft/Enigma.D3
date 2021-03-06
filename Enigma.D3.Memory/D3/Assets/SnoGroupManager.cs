using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enigma.D3.Collections;
using Enigma.D3.Enums;
using Enigma.D3.Win32;
using Enigma.D3.Memory.TypeSystem;

namespace Enigma.D3.Assets
{
	public class SnoGroupManager : SnoGroupManager<MemoryObject> { }

	public class SnoGroupManager<T> : MemoryObject
	{
		// 2.4.3.42655
		public const int SizeOf = 0x7C;

		public int x00_VTable { get { return Read<int>(0x00); } }
		public int x04 { get { return Read<int>(0x04); } }
		public int x08 { get { return Read<int>(0x08); } }
		public int x0C { get { return Read<int>(0x0C); } }
		public Container<SnoDefinition<T>> x10_Container { get { return ReadPointer<Container<SnoDefinition<T>>>(0x10).Dereference(); } }
		public Container x14_Container { get { return ReadPointer<Container>(0x14).Dereference(); } }
		public int x18_Flags { get { return Read<int>(0x18); } }
		public string x1C_Name { get { return ReadString(0x1C, 32); } }
		public SnoGroupId x3C_SnoGroupId { get { return (SnoGroupId)Read<int>(0x3C); } }
		public int x40 { get { return Read<int>(0x40); } }
		public int x44 { get { return Read<int>(0x44); } }
		public int x48 { get { return Read<int>(0x48); } }
		public int x4C { get { return Read<int>(0x4C); } }
		public int x50 { get { return Read<int>(0x50); } }
		public int x54 { get { return Read<int>(0x54); } }
		public int x58 { get { return Read<int>(0x58); } }
		public int x5C_Limit { get { return Read<int>(0x5C); } }
		public int x60_ItemSize { get { return Read<int>(0x60); } }
		public int x64_MaxCacheSize { get { return Read<int>(0x64); } }
		public int x68_EstCacheSize { get { return Read<int>(0x68); } }
		public ValueTypeDescriptor x6C_Ptr_DataType { get { return ReadPointer<ValueTypeDescriptor>(0x6C).Dereference(); } }
		public int x70_Counter { get { return Read<int>(0x70); } }
		public Struct_X74 x74 { get { return ReadPointer<Struct_X74>(0x74).Dereference(); } }
		public int x78_Neg1_InvalidSnoId { get { return Read<int>(0x78); } } // -1 or a group specific Axe_Bad_Data ID


		public class Struct_X74 : MemoryObject
		{
			public const int SizeOf = 0;

			public CriticalSection x00_CriticalSection { get { return Read<CriticalSection>(0x00); } }
			public CriticalSection x18_CriticalSection { get { return Read<CriticalSection>(0x18); } }
			public CriticalSection x30_CriticalSection { get { return Read<CriticalSection>(0x30); } }
			public int x48 { get { return Read<int>(0x48); } }
			public int x4C { get { return Read<int>(0x4C); } }
		}
	}
}
