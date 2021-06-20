using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.Cpu.Utilities.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using UnicornNET;

namespace MejorNX.Cpu.Memory
{
    public delegate void ExternalHook(params object[] values);

    public enum HookType
    {
        MappedMemory,
    }

    public unsafe class VirtualMemoryManager
    {
        public static VirtualMemoryManager MainVMM { get; private set; }

        public const ulong RamSize = 4UL * 1024 * 1024 * 1024;

        public const ulong PageBit = 12;
        public const ulong PageTableBit = 14;

        public const ulong PageSize = 1 << (int)PageBit;
        public const ulong PageTableSize = 1 << (int)PageTableBit;

        public const ulong PageMask = PageSize - 1;
        public const ulong PageTableMask = PageTableBit - 1;

        public static ulong PageRoundUp(ulong Address) => (Address + PageMask) & ~PageMask;
        public static ulong PageRoundDown(ulong Address) => Address & ~PageMask;
        public static bool IsValidPosition(ulong Position) => Position >> 32 == 0;

        public const ulong PageTableCount = RamSize / PageTableSize;
        public const ulong PageTableLength = RamSize / PageTableCount;

        public const ulong MainStackSize = 0x100000;
        public const ulong MainStackAddress = RamSize - MainStackSize;
        public const ulong StackTop = MainStackAddress + MainStackSize;

        public const ulong TlsSize = 0x20000;
        public const ulong TlsCollectionAddress = MainStackAddress - TlsSize;

        public const ulong ImageBase = 0x8000000;
        public const ulong HeapBase = 0x10000000 + 0x20000000;

        public static ulong CurrentHeapSize     { get; set; }

        public static PageEntry[][] PageTable   { get; set; }

        public static byte* BaseAddress         { get; set; }

        public static ExternalHook MemoryHook   { get; set; }

        public VirtualMemoryManager()
        {
            if (MainVMM != null)
                throw new Exception();

            MainVMM = this;

            PageTable = new PageEntry[PageTableCount][];

            MapMemory(MainStackAddress, MainStackSize, MemoryPermission.ReadAndWrite, MemoryType.Normal);
            MapMemory(TlsCollectionAddress, TlsSize, MemoryPermission.ReadAndWrite, MemoryType.ThreadLocal);

            BaseAddress = (byte*)Marshal.AllocHGlobal((IntPtr)RamSize).ToPointer();
        }

        public static (ulong, ulong) GetPointer(ulong Address)
        {
            Address >>= (int)PageBit;

            return (Address >> (int)PageTableBit, Address & ((1 << (int)PageTableBit) - 1));
        }

        public PageEntry GetPage(ulong Address)
        {
            (ulong, ulong) Pointer = GetPointer(Address);

            if (PageTable[Pointer.Item1] != null)
            {
                return PageTable[Pointer.Item1][Pointer.Item2];
            }

            return PageEntry.Null();
        }

        ref PageEntry RequestPage(ulong Address)
        {
            (ulong, ulong) Pointer = GetPointer(Address);

            if (!PageTableExists(Address))
            {
                PageTable[Pointer.Item1] = new PageEntry[PageTableLength];
            }

            return ref PageTable[Pointer.Item1][Pointer.Item2];
        }

        public bool PageTableExists(ulong Address) => PageTable[GetPointer(Address).Item1] != null;

        public void MapMemory(ulong Address, ulong Size, MemoryPermission Permission, MemoryType Type, uint Attr = 0, bool mapped = true)
        {
            Address = PageRoundDown(Address);
            Size = PageRoundUp(Size);
            ulong Top = Address + Size;

            lock (PageTable)
            {
                Debug.Log($"Mapped Memory At Address: {StringTools.FillStringBack(Address, ' ', 20)} With Size: {StringTools.FillStringBack(Size, ' ', 20)} With Permission: {StringTools.FillStringBack(Permission, ' ', 20)} With Type: {StringTools.FillStringBack((int)Type, ' ', 20)}");

                for (; Address < Top; Address += PageSize)
                {
                    RequestPage(Address).mapped = true;
                    RequestPage(Address).Permission = Permission;
                    RequestPage(Address).MemoryType = Type;
                    RequestPage(Address).Attr = Attr;
                    RequestPage(Address).mapped = mapped;
                }

                CallHook(HookType.MappedMemory, Address, Size, (int)Type, (int)Permission);
            }
        }

        public void UnmapMemory(ulong Address, ulong Size, int Type) => MapMemory(Address, Size, MemoryPermission.None, (MemoryType)Type, 0, false);

        public MapInfo GetMapInfo(ulong Address)
        {
            lock (PageTable)
            {
                ulong Bottom = PageRoundDown(Address);
                ulong Top = Bottom;

                if (!IsValidPosition(Bottom))
                    return null;

                PageEntry check = GetPage(Bottom);

                for (; Bottom >= PageSize; Bottom -= PageSize)
                {
                    PageEntry test = GetPage(Bottom - PageSize);

                    if (!PageEntry.Compare(check, test))
                    {
                        break;
                    }
                }

                for (; Top < RamSize; Top += PageSize)
                {
                    PageEntry test = GetPage(Top);

                    if (!PageEntry.Compare(check, test))
                    {
                        break;
                    }
                }

                return new MapInfo(Bottom, Top - Bottom, check.Permission, check.MemoryType, check.Attr);
            }
        }

        public void ReprotectMemory(ulong Address, ulong Size, MemoryPermission permission)
        {
            lock (PageTable)
            {
                Address = PageRoundDown(Address);
                Size = PageRoundUp(Size);
                ulong Top = Address + Size;

                Debug.Log($"Protected Memory At Address: {StringTools.FillStringBack(Address, ' ', 20)} With Size: {StringTools.FillStringBack(Size, ' ', 20)} With Permission: {StringTools.FillStringBack(permission, ' ', 20)}");

                for (; Address < Top; Address += PageSize)
                {
                    RequestPage(Address).Permission = permission;
                }
            }
        }

        public void SetAttrBit(ulong Address, ulong Size, int bit)
        {
            lock (PageTable)
            {
                Address = PageRoundDown(Address);
                Size = PageRoundUp(Size);
                ulong Top = Address + Size;

                for (; Address < Top; Address += PageSize)
                {
                    RequestPage(Address).Attr |= 1U << bit;
                }
            }
        }

        public void ClearAttrBit(ulong Address, ulong Size, int bit)
        {
            lock (PageTable)
            {
                Address = PageRoundDown(Address);
                Size = PageRoundUp(Size);
                ulong Top = Address + Size;

                for (; Address < Top; Address += PageSize)
                {
                    uint Mask = ~(uint)(1 << bit);

                    RequestPage(Address).Attr &= Mask;
                }
            }
        }
        public static MemoryWriter GetWriter(ulong Address = 0)
        {
            MemoryWriter Out = new MemoryWriter(BaseAddress);

            Out.Seek(Address);

            return Out;
        }

        public static MemoryReader GetReader(ulong Address = 0)
        {
            MemoryReader Out = new MemoryReader(BaseAddress);

            Out.Seek(Address);

            return Out;
        }

        public void CallHook(params object[] arguments)
        {
            if (MemoryHook != null)
                MemoryHook(arguments);
        }

        public void FillZeros(ulong Address, ulong Size)
        {
            for (ulong i = 0; i < Size; i++)
            {
                BaseAddress[i + Address] = 0;
            }
        }

        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}
