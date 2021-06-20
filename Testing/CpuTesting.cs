/*

using GalaxicARM.Runtime;
using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MejorNX.Testing
{
    public class CpuTesting
    {
        public ArmThread thread     { get; set; }
        public CpuTesting()
        {
            thread = AContext.CreateThread();
        }

        public unsafe void TestProgram(byte[] Program)
        {
            byte* addr = MemoryTools.GetPointer(Program);

            GCHandle.Alloc(Program,GCHandleType.Pinned);

            AContext.BasePointer = addr;

            thread.Execute();
        }
    }
}
*/