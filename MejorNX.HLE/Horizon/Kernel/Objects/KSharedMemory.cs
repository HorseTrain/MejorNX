using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KSharedMemory
    {
        public uint Handle                      { get; set; }

        public List<ulong> VirtualAddresses     { get; set; }

        public void OpenToHandle(Process process)
        {
            Handle = process.ServiceHandles.AddObject(this);

            VirtualAddresses = new List<ulong>();
        }

        public void AddVirtualAddress(ulong Address) => VirtualAddresses.Add(Address);

        public bool Valid => VirtualAddresses.Count != 0;
    }
}
