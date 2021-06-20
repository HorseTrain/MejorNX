using System;
using System.Runtime.InteropServices;

namespace MejorNX.Cpu.Memory
{
    public unsafe class MemoryWriter : Stream
    {
        public MemoryWriter(void* Location) : base(Location)
        {

        }

        public void WriteStruct<T>(T data) where T: unmanaged
        {
            *(T*)CurrentLocation = data;

            Advance((ulong)sizeof(T));
        }

        public void WriteStruct<T>(ulong Address, T data) where T : unmanaged
        {
            Seek(Address);

            *(T*)CurrentLocation = data;

            Advance((ulong)sizeof(T));
        }

        public void WriteStruct<T>(T[] data) where T: unmanaged
        {
            fixed (T* dat = data)
            {
                ulong size = (uint)data.Length * (uint)sizeof(T);

                Buffer.MemoryCopy(dat, CurrentLocation, size, size);
            }

            Advance((ulong)data.Length * (ulong)sizeof(T));
        }

        public void WriteStruct<T>(ulong offset, T[] data) where T : unmanaged
        {
            Seek(offset);

            WriteStruct(data);
        }
    }
}
