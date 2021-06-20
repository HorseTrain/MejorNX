using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace MejorNX.Cpu.Memory
{
    public unsafe class BinaryBuilder
    {
        List<byte> Buffer   { get; set; }
        ulong Position      { get; set; }

        public BinaryBuilder()
        {
            Buffer = new List<byte>();
        }

        public void WriteStruct<T>(T dat) where T: unmanaged
        {
            byte* loc = (byte*)&dat;

            for (int i = 0; i < sizeof(T); i++)
            {
                WriteByte(loc[i]);
            }
        }

        public byte[] GetBuffer() => Buffer.ToArray();

        public void WriteByte(byte b)
        {
            while (Position >= (ulong)Buffer.Count)
            {
                Buffer.Add(0);
            }

            Buffer[(int)Position] = b;

            Position++;
        }

        public void Seek(ulong Position)
        {
            this.Position = Position;
        }

        public void Write(byte[] Buffer)
        {
            for (int i = 0; i < Buffer.Length; i++)
                WriteByte(Buffer[i]);
        }
    }
}
