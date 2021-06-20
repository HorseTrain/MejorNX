using MejorNX.HLE.Horizon.IPC.Descriptors;
using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MejorNX.HLE.Horizon.IPC
{
    public unsafe class IPCCommand
    {
        public ulong Address                                    { get; set; }
        public bool IsDomain                                    { get; set; }
        public CommandType Type                                 { get; set; }

        public HandleDescriptor         HandleDescriptor        { get; set; }

        public List<PointerDescriptor>  PointerDescriptors      { get; set; } = new List<PointerDescriptor>();
        public List<SREDescriptor>      SendDescriptors         { get; set; } = new List<SREDescriptor>();
        public List<SREDescriptor>      ReceiveDescriptors      { get; set; } = new List<SREDescriptor>();
        public List<SREDescriptor>      ExchangeDescriptors     { get; set; } = new List<SREDescriptor>();
        public List<ReceiveListDescriptor> ReceiveLists         { get; set; } = new List<ReceiveListDescriptor>();

        public List<int>                Responses               { get; set; } = new List<int>();

        public DomainCommand            DCommand                { get; set; }
        public ulong                    DID                     { get; set; }

        public byte[]                   RawData                 { get; set; }
        public ulong                    RawDataPointer          { get; set; }
        public ulong                    RawDataSize =>          (ulong)RawData.Length;

        public bool                     Ignore                  { get; set; }

        public void CreateBuffer(List<SREDescriptor> descriptors,MemoryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                descriptors.Add(new SREDescriptor(reader));
            }
        }

        public IPCCommand(MemoryReader reader)
        {
            Address = reader.Location;

            int word0 = reader.ReadStruct<int>();
            int word1 = reader.ReadStruct<int>();

            Type = (CommandType)(word0 & 0xFFFF);
            int PointerCount = (word0 >> 16) & 0xF;
            int SendCount = (word0 >> 20) & 0xF;
            int RecieveCount = (word0 >> 24) & 0xF;
            int ExchangeCount = (word0 >> 28) & 0xF;

            int SizeOfRawData = (word1 & 0x3FF) * 4;
            int CDescriptorFlags = (word1 >> 10) & 0xF;
            bool EnableHandleDescriptor = ((word1 >> 31) & 1) == 1;

            if (EnableHandleDescriptor)
            {
                HandleDescriptor = new HandleDescriptor(reader);
            }

            for (int i = 0; i < PointerCount; ++i)
            {
                PointerDescriptors.Add(new PointerDescriptor(reader));
            }

            CreateBuffer(SendDescriptors,reader,SendCount);
            CreateBuffer(ReceiveDescriptors, reader, RecieveCount);
            CreateBuffer(ExchangeDescriptors,reader,ExchangeCount);

            ulong RecvListPos = reader.Location + (ulong)SizeOfRawData;

            ulong pad0 = Pad16(reader.Location + (ulong)reader.BaseLocation);

            reader.Advance(pad0);

            long RecvListCount = (long)CDescriptorFlags - 2;

            if (RecvListCount == 0)
                RecvListCount = 1;
            else if (RecvListCount < 0)
                RecvListCount = 0;

            if (IsDomain)
            {
                int Dword = reader.ReadStruct<int>();

                DCommand = (DomainCommand)(Dword & 0xff);

                SizeOfRawData = (Dword >> 16) & 0xffff;

                DID = reader.ReadStruct<uint>();

                reader.ReadStruct<ulong>();
            }

            RawDataPointer = reader.Location;
            RawData = reader.ReadArray<byte>((ulong)SizeOfRawData);

            reader.Seek(RecvListPos);

            for (long i = 0; i < RecvListCount; i++)
            {
                ReceiveLists.Add(new ReceiveListDescriptor(reader));
            }
        }

        public IPCCommand()
        {

        }

        static ulong Pad16(ulong Position)
        {
            if ((Position & 0xf) != 0)
            {
                return 0x10 - (Position & 0xf);
            }

            return 0;
        }

        public byte[] BuildResponse(ulong Address)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                int Word0 = ((int)Type) | (((PointerDescriptors.Count & 0xf) << 16) | ((SendDescriptors.Count & 0xf) << 20) | ((ReceiveDescriptors.Count & 0xf) << 24) | ((ExchangeDescriptors.Count & 0xf) << 28));
                int Word1;

                byte[] HandleData = new byte[0];

                if (HandleDescriptor != null)
                {
                    HandleData = HandleDescriptor.GenerateResponse();
                }

                int DataLength = RawData?.Length ?? 0;

                int Pad0 = (int)Pad16(Address + 8UL + (ulong)HandleData.Length);

                int Pad1 = 0x10 - Pad0;

                DataLength = (DataLength + Pad0 + Pad1 + (IsDomain ? 0x10 : 0)) / 4;

                DataLength += Responses.Count;

                Word1 = DataLength & 0x3ff;

                if (HandleDescriptor != null)
                {
                    Word1 |= 1 << 31;
                }

                Writer.Write(Word0);
                Writer.Write(Word1);
                Writer.Write(HandleData);

                MS.Seek(Pad0, SeekOrigin.Current);

                if (IsDomain)
                {
                    Writer.Write(Responses.Count);
                    Writer.Write(0);
                    Writer.Write(0L);
                }

                if (RawData != null)
                {
                    Writer.Write(RawData);
                }

                foreach (int Id in Responses)
                {
                    Writer.Write(Id);
                }

                Writer.Write(new byte[Pad1]);

                return MS.ToArray(); 
            }
        }

        public ulong GetSendBuffPtr()
        {
            if (SendDescriptors.Count > 0 && SendDescriptors[0].Address != 0)
            {
                return SendDescriptors[0].Address;
            }

            if (PointerDescriptors.Count > 0 && PointerDescriptors[0].Address != 0)
            {
                return PointerDescriptors[0].Address;
            }

            return ulong.MaxValue;
        }

        public (ulong Position, ulong Size) GetBufferType0x21()
        {
            if (PointerDescriptors.Count != 0 && PointerDescriptors[0].IsNotZero)
            {
                return (PointerDescriptors[0].Address, PointerDescriptors[0].Size);
            }

            return (0, 0);
        }

        public (ulong Position, ulong Size) GetBufferType0x22()
        {
            if (ReceiveLists.Count != 0 && ReceiveLists[0].IsNotZero)
            {
                return (ReceiveLists[0].Address, ReceiveLists[0].Size);
            }

            return (0, 0);
        }
    }
}
