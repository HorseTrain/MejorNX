using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public enum MemoryPermission
    {
        None = 0,
        Read = 1 << 0,
        Write = 1 << 1,
        Execute = 1 << 2,

        ReadAndWrite = Read | Write,
        ReadAndExecute = Read | Execute
    }
}
