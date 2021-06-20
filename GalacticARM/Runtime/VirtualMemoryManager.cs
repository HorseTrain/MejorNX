using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime
{
    public enum MemoryAccess : byte
    {
        Read    = 1 << 0,
        Write   = 1 << 1,
        Execute = 1 << 2,
        All = Read | Write | Execute
    }

    public unsafe struct PageInfo
    {
        public ulong PhysicalAddress;
        public ulong Meta; 

        public void Reset()
        {
            Meta = 0;
        }
    }

    public struct RegionInfo
    {
        public PageInfo MapType { get; set; }
        public ulong Base       { get; set; }
        public ulong Size       { get; set; }
    }

    public unsafe struct MemoryMap
    {
        public ulong VirtualAddress;
        public ulong Size;

        public void* PhysicalAddress;
    }

    public static unsafe class VirtualMemoryManager
    {
        public const int PageBit = 12;
        const ulong PageSize = 1 << PageBit;
        public const ulong PageMask = PageSize - 1;
        const int AddressSpaceSize = 39;

        public static ulong PageMapCount;

        public static PageInfo* PageMap;

        public static List<MemoryMap> Maps { get; set; }

        static unsafe VirtualMemoryManager()
        {
            PageMapCount = ((1UL << AddressSpaceSize) >> PageBit);

            ulong[] b = new ulong[(PageMapCount * (ulong)sizeof(PageInfo)) >> 3];

            GCHandle.Alloc(b,GCHandleType.Pinned);

            fixed (ulong* t = b)
            {
                PageMap = (PageInfo*)t;
            }

            Maps = new List<MemoryMap>();
        }

        public static void* ReqeustPhysicalAddress(ulong VirtualAddress, MemoryAccess RequestType = MemoryAccess.All)
        {
            ulong Index = VirtualAddress >> PageBit;
            ulong Offset = VirtualAddress & PageMask;

            PageInfo Info = PageMap[Index];

            return (byte*)Info.PhysicalAddress + Offset;
        }

        public static void MapMemory(ulong VirtualAddress, void* PhysicalAddress, ulong Size, MemoryAccess Access = MemoryAccess.All, byte Type = 0)
        {
            ulong Bottom = VirtualAddress & ~PageMask;
            ulong Top = (Bottom + Size + PageMask) & ~PageMask;

            ulong Offset = (ulong)PhysicalAddress;

            for (; Bottom < Top; Bottom += PageSize)
            {
                ulong Index = Bottom >> PageBit;

                PageMap[Index].PhysicalAddress = Offset;
                //PageMap[Index].Access = Access;
                //PageMap[Index].Type = Type;
                //PageMap[Index].Mapped = true;

                Offset += PageSize;
            }

            lock (Maps)
            {
                Maps.Add(new MemoryMap() { VirtualAddress = VirtualAddress, Size = Size, PhysicalAddress = PhysicalAddress });
            }
        }

        public static T ReadObject<T>(ulong VirtualAddress) where T: unmanaged
        {
            return *(T*)ReqeustPhysicalAddress(VirtualAddress,MemoryAccess.Read);
        }

        public static T[] ReadObjects<T>(ulong VirtualAddress, int size) where T : unmanaged
        {
            T[] Out = new T[size];

            for (int i = 0; i < size; i++)
            {
                Out[i] = ReadObject<T>(VirtualAddress + (ulong)i);
            }

            return Out;
        }

        public static void WriteObject<T>(ulong VirtualAddress, T data) where T: unmanaged
        {
            *(T*)ReqeustPhysicalAddress(VirtualAddress, MemoryAccess.Write) = data;
        }

        public static string GetOpHex(ulong Address)
        {
            UInt32 ReverseBytes(UInt32 value)
            {
                return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                       (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
            }

            return $"{ReverseBytes(ReadObject<uint>(Address)):x8}";
        }
    }

    public static unsafe class ExclusiveMonitors
    {
        public const ulong ErgMask = (4 << 4) - 1;

        public struct Monitor
        {
            public ulong Address;
            public bool ExState;

            public Monitor(ulong Address, bool ExState)
            {
                this.Address = Address;
                this.ExState = ExState;
            }

            public bool HasExclusiveAccess(ulong Address)
            {
                return this.Address == Address && ExState;
            }

            public void Reset()
            {
                ExState = false;
            }
        }

        public static HashSet<ulong> Addresses { get; set; }

        public static Dictionary<ulong, Monitor> Monitors { get; set; }

        static ExclusiveMonitors()
        {
            Addresses = new HashSet<ulong>();
            Monitors = new Dictionary<ulong, Monitor>();
        }

        public static void SetExclusive(ExecutionContext* context, ulong Address)
        {
            Address &= ~ErgMask;

            lock (Monitors)
            {
                if (Monitors.TryGetValue(context->ID, out Monitor monitor))
                {
                    Addresses.Remove(monitor.Address);
                }

                bool ExState = Addresses.Add(Address);

                Monitor Monitor = new Monitor(Address, ExState);

                if (!Monitors.TryAdd(context->ID, Monitor))
                {
                    Monitors[context->ID] = Monitor;
                }
            }
        }

        public static ulong TestExclusive(ExecutionContext* context, ulong Address)
        {
            Address &= ~ErgMask;
            lock (Monitors)
            {
                if (!Monitors.TryGetValue(context->ID, out Monitor monitor))
                {
                    return 0;
                }

                return monitor.HasExclusiveAccess(Address) ? 1UL : 0;
            }
        }

        public static void Clrex(ExecutionContext* context)
        {
            lock (Monitors)
            {
                if (Monitors.TryGetValue(context->ID, out Monitor monitor))
                {
                    monitor.Reset();
                    Addresses.Remove(monitor.Address);
                }
            }
        }
    }
}
