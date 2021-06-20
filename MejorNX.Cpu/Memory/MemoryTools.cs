using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public static unsafe class MemoryTools
    {
        public static T* GetPointer<T>(T[] Data) where T: unmanaged
        {
            fixed (T* dat = Data)
                return dat;
        }
    }
}
