using System;
using System.Collections.Concurrent;

namespace MejorNX.Maxwell
{
    //Would it make more sense to place this in hle ?
    public unsafe class MaxwellVirtualMemoryManager
    {
        public byte* BaseAddress        { get; set; }

        public const ulong RamSize = 4UL * 1024 * 1024 * 1024;

        public const ulong PageBit = 12;
        public const ulong PageTableBit = 14;

        public const ulong PageSize = 1 << (int)PageBit;
        public const ulong PageTableSize = 1 << (int)PageTableBit;

        public const ulong PageMask = PageSize - 1;
        public const ulong PageTableMask = PageTableBit - 1;

        public const ulong PageTableCount = RamSize / PageTableSize;
        public const ulong PageTableLength = RamSize / PageTableCount;

        public static ulong PageRoundUp(ulong Address) => (Address + PageMask) & ~PageMask;
        public static ulong PageRoundDown(ulong Address) => Address & ~PageMask;

        public static ulong[][] PageTable { get; set; }

        public const ulong Unmapped = ulong.MaxValue;
        public const ulong Reservec = ulong.MaxValue - 1;

        public MaxwellVirtualMemoryManager(void* BaseAddress)
        {
            this.BaseAddress = (byte*)BaseAddress;

            PageTable = new ulong[PageTableCount][];
        }

        static (ulong, ulong) GetPointer(ulong Address)
        {
            Address >>= (int)PageBit;

            return (Address >> (int)PageTableBit, Address & ((1 << (int)PageTableBit) - 1));
        }

        public ulong GetPage(ulong Address)
        {
            (ulong, ulong) Pointer = GetPointer(Address);

            if (PageTable[Pointer.Item1] != null)
            {
                return PageTable[Pointer.Item1][Pointer.Item2];
            }

            return Unmapped;
        }

        public bool PageTableExists(ulong Address) => PageTable[GetPointer(Address).Item1] != null;

        ref ulong RequestPage(ulong Address)
        {
            (ulong, ulong) Pointer = GetPointer(Address);

            if (!PageTableExists(Address))
            {
                PageTable[Pointer.Item1] = new ulong[PageTableLength];

                for (int i = 0; i < PageTable[Pointer.Item1].Length; i++)
                {
                    PageTable[Pointer.Item1][i] = Unmapped;
                }
            }

            return ref PageTable[Pointer.Item1][Pointer.Item2];
        }

        public void MapMemory(ulong Address, ulong Size, ulong PhysicalAddress)
        {
            Address = PageRoundDown(Address);
            Size = PageRoundUp(Size);
            ulong Top = Address + Size;
            ulong Offset = 0;

            lock (PageTable)
            {
                for (; Address < Top; Address += PageSize)
                {
                    RequestPage(Address) = PhysicalAddress + Offset;

                    Offset += PageSize;
                }
            }
        }

        ulong AllocationTop = RamSize;

        public ulong Map(ulong Size, ulong PhysicalAddress)
        {
            Size = PageRoundUp(Size);

            ulong Out = AllocationTop;

            AllocationTop -= Size;

            MapMemory(AllocationTop,Size,PhysicalAddress);

            return Out;
        }

        public ulong GetPhysicalAddress(ulong VirtualAddress) => RequestPage(VirtualAddress) + (VirtualAddress & PageMask);
    }
}
