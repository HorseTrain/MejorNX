using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC.Descriptors;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MejorNX.HLE.Horizon.Service.fspsrv
{
    public class IStorage : ICommand
    {
        FileStream Base { get; set; }

        public IStorage()
        {
            Calls = new System.Collections.Generic.Dictionary<ulong, ServiceCall>()
            {
                {0,     Read},
            };
        }

        unsafe ulong Read(ServiceCallContext context)
        {
            ulong Offset = context.Reader.ReadStruct<ulong>();
            ulong Size = context.Reader.ReadStruct<ulong>();

            if (context.Request.ReceiveDescriptors.Count > 0)
            {
                SREDescriptor descriptor = context.Request.ReceiveDescriptors[0];

                if (Size > descriptor.Size)
                {
                    Size = descriptor.Size;
                }

                //Is size ever over int.MaxValue?
                byte[] Data = new byte[Size];

                Base.Seek((long)Offset,SeekOrigin.Begin);
                Base.Read(Data,0,Data.Length);

                Marshal.Copy(Data,0,(IntPtr)(VirtualMemoryManager.BaseAddress + descriptor.Address),Data.Length);

                //VirtualMemoryManager.GetWriter(descriptor.Address).WriteStruct(Data);
            }

            return 0;
        }

        public override void InitData(object obj)
        {
            Base = (FileStream)obj;
        }
    }
}
