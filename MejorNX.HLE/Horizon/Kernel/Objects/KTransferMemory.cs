using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KTransferMemory : KObject
    {
        public MemoryPermission Permission      { get; set; }
        public ulong Address                    { get; set; }
        public ulong Size                       { get; set; }

        public KTransferMemory(Process process, ulong Address, ulong Size, MemoryPermission Permission) : base (process)
        {
            Handle = process.ServiceHandles.AddObject(this);

            this.Address = Address;
            this.Size = Size;
            this.Permission = Permission;
        }
    }
}
