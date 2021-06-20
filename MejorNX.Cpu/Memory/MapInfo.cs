using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public class MapInfo
    {
        public ulong BaseAddress            { get; set; }
        public ulong Size                   { get; set; }
        public MemoryPermission Permission  { get; set; }
        public MemoryType Type              { get; set; }
        public uint Attr                    { get; set; }

        public MapInfo(ulong BaseAddress, ulong Size, MemoryPermission Permission, MemoryType Type, uint Attr)
        {
            this.BaseAddress = BaseAddress;
            this.Size = Size;
            this.Permission = Permission;
            this.Type = Type;
            this.Attr = Attr;
        }
    }
}
