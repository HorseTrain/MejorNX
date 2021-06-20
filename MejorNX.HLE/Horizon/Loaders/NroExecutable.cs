using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.HLE.Horizon.Loaders
{
    //<-- https://switchbrew.org/wiki/NRO -->
    public unsafe class NroExecutable : IExecutable
    {
        public CodeSource Text      { get; set; }
        public CodeSource RoData    { get; set; }
        public CodeSource Data      { get; set; }

        public uint Mod0Offset      { get; set; }
        public uint BssSize         { get; set; }

        public ulong Size => (ulong)(Text.Data.Length + RoData.Data.Length + Data.Data.Length);

        struct NroCodeSource
        {
            public uint Offset { get; set; }
            public uint Size { get; set; }
        }

        public NroExecutable(byte[] Source)
        {
            BinaryReader reader = new BinaryReader(Source);

            Text = new CodeSource();
            RoData = new CodeSource();
            Data = new CodeSource();

            Mod0Offset = reader.ReadStructAtOffset<uint>(0x4);

            reader.Seek(32);

            LoadSource(Text, reader.ReadStruct<NroCodeSource>(),reader);
            LoadSource(RoData, reader.ReadStruct<NroCodeSource>(), reader);
            LoadSource(Data, reader.ReadStruct<NroCodeSource>(), reader);
            BssSize = reader.ReadStruct<uint>();
        }

        void LoadSource(CodeSource Des, NroCodeSource Source,BinaryReader reader)
        {
            Des.Offset = Source.Offset;
            Des.Size = Source.Size;

            Des.Data = reader.ReadRange(Source.Offset,Source.Size);
        }
    }
}
