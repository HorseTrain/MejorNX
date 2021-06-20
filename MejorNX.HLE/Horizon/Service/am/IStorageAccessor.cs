using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IStorageAccessor : ICommand
    {
        amIStorage Storage;

        public IStorageAccessor()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0,  GetSize},
                {10, Write },
                {11, Read }
            };
        }

        public override void InitData(object obj)
        {
            Storage = (amIStorage)obj;
        }

        ulong GetSize(ServiceCallContext context)
        {
            context.Writer.WriteStruct((ulong)Storage.Data.Length);

            return 0;
        }

        ulong Write(ServiceCallContext context)
        {
            ulong WritePosition = context.Reader.ReadStruct<ulong>();

            (ulong Position, ulong Size) = context.Request.GetBufferType0x21();

            if (Size > 0)
            {
                ulong MaxSize = (ulong)Storage.Data.Length - WritePosition;

                if (Size > MaxSize)
                {
                    Size = MaxSize;
                }

                byte[] Data = VirtualMemoryManager.GetReader(Position).ReadArray<byte>(Size);

                Buffer.BlockCopy(Data, 0, Storage.Data, (int)WritePosition, (int)Size);
            }

            return 0;
        }

        ulong Read(ServiceCallContext context)
        {
            ulong ReadPosition = context.Reader.ReadStruct<ulong>(); //Why lol

            (ulong Position, ulong Size) = context.Request.GetBufferType0x22();

            byte[] Data;

            if ((ulong)Storage.Data.Length > Size)
            {
                Data = new byte[Size];

                Buffer.BlockCopy(Storage.Data, 0, Data, 0, (int)Size);
            }
            else
            {
                Data = Storage.Data;
            }

            VirtualMemoryManager.GetWriter(Position).WriteStruct(Data);

            return 0;
        }
    }
}
