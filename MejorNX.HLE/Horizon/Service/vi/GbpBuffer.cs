using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public struct GbpBuffer
    {
        public int Magic { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Stride { get; private set; }
        public int Format { get; private set; }
        public int Usage { get; private set; }

        public int Pid { get; private set; }
        public int RefCount { get; private set; }

        public int FdsCount { get; private set; }
        public int IntsCount { get; private set; }

        public byte[] RawData { get; private set; }

        public int Size => RawData.Length + 10 * 4;

        public GbpBuffer(MemoryReader Reader)
        {
            Magic = Reader.ReadStruct<int>();
            Width = Reader.ReadStruct<int>();
            Height = Reader.ReadStruct<int>();
            Stride = Reader.ReadStruct<int>();
            Format = Reader.ReadStruct<int>();
            Usage = Reader.ReadStruct<int>();

            Pid = Reader.ReadStruct<int>();
            RefCount = Reader.ReadStruct<int>();

            FdsCount = Reader.ReadStruct<int>();
            IntsCount = Reader.ReadStruct<int>();

            RawData = Reader.ReadArray<byte>((ulong)((FdsCount + IntsCount) * 4));
        }

        public void Write(BinaryBuilder Writer)
        {
            Writer.WriteStruct(Magic);
            Writer.WriteStruct(Width);
            Writer.WriteStruct(Height);
            Writer.WriteStruct(Stride);
            Writer.WriteStruct(Format);
            Writer.WriteStruct(Usage);

            Writer.WriteStruct(Pid);
            Writer.WriteStruct(RefCount);

            Writer.WriteStruct(FdsCount);
            Writer.WriteStruct(IntsCount);

            Writer.Write(RawData);
        }
    }
}
