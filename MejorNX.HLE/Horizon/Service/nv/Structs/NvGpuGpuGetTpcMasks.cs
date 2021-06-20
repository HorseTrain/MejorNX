using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvGpuGpuGetTpcMasks
    {
        public int MaskBufferSize;
        public int Reserved;
        public long MaskBufferAddress;
        public int TpcMask;
        public int Padding;
    }
}
