using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public static class Allocator
    {
        public static ulong CurrentP    { get; set; }

        public static ulong Allocate(ulong Size)
        {
            Size = VirtualMemoryManager.PageRoundUp(Size);

            ulong Out = CurrentP;

            CurrentP += Size;

            return Out;
        }
    }
}
