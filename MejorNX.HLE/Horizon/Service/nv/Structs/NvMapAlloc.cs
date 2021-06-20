using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvMapAlloc
    {
        public int Handle;
        public int HeapMask;
        public int Flags;
        public int Align;
        public ulong Kind;
        public ulong Address;
    }
}
