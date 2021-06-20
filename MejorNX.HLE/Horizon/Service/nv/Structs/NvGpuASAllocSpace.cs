using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvGpuASAllocSpace
    {
        public int Pages;
        public int PageSize;
        public int Flags;
        public int Padding;
        public long Offset;
    }
}
