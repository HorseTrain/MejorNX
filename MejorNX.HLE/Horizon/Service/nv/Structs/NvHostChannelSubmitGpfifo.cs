using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Structs
{
    public struct NvHostChannelSubmitGpfifo
    {
        public long Gpfifo;
        public int NumEntries;
        public int Flags;
        public int SyncptId;
        public int SyncptValue;
    }
}
