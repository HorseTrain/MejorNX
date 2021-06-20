using System;
using System.Collections.Generic;
using System.Text;
using MejorNX.Cpu.Memory;

namespace MejorNX.HLE.Horizon.IPC.Descriptors
{
    public class PointerDescriptor
    {
        public ulong Counter { get; set; }
        public ulong Address { get; set; }
        public ulong Size { get; set; }

        public bool IsNotZero => Address != 0 && Size != 0;

        public PointerDescriptor(MemoryReader reader)
        {
            uint word0 = reader.ReadStruct<uint>();
            uint word1 = reader.ReadStruct<uint>();

            Address = word1;
            Address |= (word1 << 20) & 0x0f00000000UL;
            Address |= (word1 << 30) & 0x7000000000UL;

            Counter |= (word0 & 0x3F);
            Counter |= (word0 & 0xE00);

            Size = (ushort)(word0 >> 16);
        }
    }
}
