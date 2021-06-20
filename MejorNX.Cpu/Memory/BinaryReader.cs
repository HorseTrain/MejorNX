using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.Cpu.Memory
{
    public unsafe class BinaryReader : MemoryReader
    {
        GCHandle handle;

        public BinaryReader(byte[] Buffer) : base(MemoryTools.GetPointer(Buffer))
        {
            handle = GCHandle.Alloc(Buffer,GCHandleType.Pinned);
        }

        ~BinaryReader()
        {
            handle.Free();
        }

        public ulong TellG() => ((ulong)CurrentLocation - (ulong)BaseLocation);

        public byte[] ReadRange(ulong Offset, ulong Size)
        {
            ulong Temp = TellG();

            Seek(Offset);

            byte[] Out = ReadArray<byte>(Size);

            Seek(Temp);

            return Out;
        }
    }
}
