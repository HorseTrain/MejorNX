using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public delegate ulong ServiceProcessParcel(ServiceCallContext context, MemoryReader parcelreader);
}
