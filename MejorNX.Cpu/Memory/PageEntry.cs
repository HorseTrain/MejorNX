using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public struct PageEntry
    {
        public MemoryPermission Permission;
        public MemoryType MemoryType;
        public uint Attr;
        public bool mapped; //might not be needed ?

        public static bool Compare(PageEntry left, PageEntry right)
        {
            return
                left.Permission == right.Permission &&
                left.MemoryType == right.MemoryType &&
                left.mapped == right.mapped &&
                left.Attr == right.Attr;
        }

        public static PageEntry Null()
        {
            return default(PageEntry);
        }
    }
}
