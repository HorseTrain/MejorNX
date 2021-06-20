using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.hid
{
    public class IAppletResource : ICommand
    {
        public KSharedMemory SharedMemory   { get; set; }

        public IAppletResource()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, GetSharedMemoryHandle }
            };
        }

        public override void InitData(object obj)
        {
            SharedMemory = (KSharedMemory)obj;
        }

        ulong GetSharedMemoryHandle(ServiceCallContext context)
        {
            SharedMemory.OpenToHandle(context.process);

            context.Response.HandleDescriptor = HandleDescriptor.MakeCopy(SharedMemory.Handle);

            return 0;
        }
    }
}
