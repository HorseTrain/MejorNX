using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvGpuGpuZcullGetInfo
    {
        public int WidthAlignPixels;
        public int HeightAlignPixels;
        public int PixelSquaresByAliquots;
        public int AliquotTotal;
        public int RegionByteMultiplier;
        public int RegionHeaderSize;
        public int SubregionHeaderSize;
        public int SubregionWidthAlignPixels;
        public int SubregionHeightAlignPixels;
        public int SubregionCount;
    }
}
