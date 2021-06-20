using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.aud.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioOutData
    {
        public long NextBufferPtr;
        public long SampleBufferPtr;
        public long SampleBufferCapacity;
        public long SampleBufferSize;
        public long SampleBufferInnerOffset;
    }
}
