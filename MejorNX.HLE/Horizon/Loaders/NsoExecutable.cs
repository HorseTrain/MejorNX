using MejorNX.Cpu.Memory;
using MejorNX.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.HLE.Horizon.Loaders
{
    //<-- https://switchbrew.org/wiki/NSO -->
    public class NsoExecutable : IExecutable
    {
        public CodeSource Text      { get; set; }
        public CodeSource RoData    { get; set; }
        public CodeSource Data      { get; set; }
        
        public uint Mod0Offset      { get; set; }
        public uint BssSize         { get; set; }
        public ulong Size => (ulong)(Text.Data.Length + RoData.Data.Length + Data.Data.Length);

        struct NsoSegmentHeader
        {
            public uint FileOffset;
            public uint MemoryOffset;
            public uint DecompressedSize;

            public override string ToString()
            {
                return $"{FileOffset} , {MemoryOffset} , {DecompressedSize}";
            }
        }

        public NsoExecutable(byte[] Source)
        {
            BinaryReader reader = new BinaryReader(Source);

            Text = new CodeSource();
            RoData = new CodeSource();
            Data = new CodeSource();

            reader.Seek(0xC);
            uint Flags = reader.ReadStruct<uint>();

            reader.Seek(0x60);
            uint TextSize = reader.ReadStruct<uint>();
            uint RoSize = reader.ReadStruct<uint>();
            uint DataSize = reader.ReadStruct<uint>();

            reader.Seek(0x10);

            WriteSourceToProgram(reader.ReadStruct<NsoSegmentHeader>(), (Flags & 1) == 0,           TextSize,   reader,     Text);

            reader.Advance(4);

            WriteSourceToProgram(reader.ReadStruct<NsoSegmentHeader>(), ((Flags >> 1) & 1) == 0,    RoSize,     reader,     RoData);

            reader.Advance(4);

            WriteSourceToProgram(reader.ReadStruct<NsoSegmentHeader>(), ((Flags >> 3) & 1) == 0,    DataSize,   reader,     Data);

            reader.Seek(0x3C);

            BssSize = reader.ReadStruct<uint>();

            reader = new BinaryReader(Text.Data);

            Mod0Offset = reader.ReadStructAtOffset<uint>(0x4);
        }

        void WriteSourceToProgram(NsoSegmentHeader Header, bool Compressed,uint readsize, BinaryReader reader,CodeSource source)
        {
            ulong LastPosition = reader.TellG();

            reader.Seek(Header.FileOffset);

            source.Offset = Header.MemoryOffset;
            source.Size = Header.DecompressedSize;
            source.Data = GetNsoCodeSource(reader.ReadArray<byte>(readsize), true, (int)Header.DecompressedSize);

            reader.Seek(LastPosition);
        }

        static byte[] GetNsoCodeSource(byte[] Buffer, bool Compressed,int DecompressedSize)
        {
            if (Compressed)
            {
                Buffer = LZ4.Decompress(Buffer, DecompressedSize);
            }

            return Buffer;
        }
    }
}
