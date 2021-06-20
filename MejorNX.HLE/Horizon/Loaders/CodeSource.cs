using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.HLE.Horizon.Loaders
{
    public class CodeSource
    {
        public uint Offset { get; set; }
        public uint Size   { get; set; }

        public byte[] Data { get; set; }

        public override string ToString()
        {
            return $"{Offset} , {Size}";
        }

        public ulong Length => (ulong)Data.Length;
    }
}
