using System;
using System.Collections.Generic;
using System.Text;
using MejorNX.Cpu.Memory;

namespace MejorNX.HLE.Horizon.IPC.Descriptors
{
    public class SREDescriptor
    {
        public ulong Size       { get; set; }
        public ulong Address    { get; set; }
        public ulong Flag       { get; set; }

        public SREDescriptor(MemoryReader reader)
        {
            uint word0 = reader.ReadStruct<uint>();
            uint word1 = reader.ReadStruct<uint>();
            uint word2 = reader.ReadStruct<uint>();

            Address = word1;
            Address |= (word2 << 4) & 0x0f00000000UL;
            Address |= (word2 << 34) & 0x7000000000UL;

            Size = word0;
            Size |= (word2 << 8) & 0xf00000000UL;

            Flag = word2 & 0x3;
        }
    }
}
