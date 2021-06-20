using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvGpuASMapBufferEx
    {
        public int Flags;
        public int Kind;
        public int NvMapHandle;
        public int PageSize;
        public ulong BufferOffset;
        public ulong MappingSize;
        public ulong Offset;
    }
}
