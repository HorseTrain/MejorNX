using MejorNX.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Objects
{
    public class NvMap
    {
        public static ObjectCollection Handles  { get; set; } = new ObjectCollection();

        public static NvMap GetMap(uint Index) => (NvMap)Handles[Index];

        public bool Allocated   { get; set; }
        public ulong Aling      { get; set; }
        public byte Kind        { get; set; }
        public ulong Address    { get; set; }
        public uint Size        { get; set; }
        public uint Handle      { get; set; }

        public NvMap(uint Size = 0)
        {
            this.Size = Size;

            Handle = Handles.AddObject(this);
        }
    }
}
