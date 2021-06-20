using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.sm
{
    public class IUserInterface : ICommand
    {
        bool IsInitialized;

        public IUserInterface()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Initialize },
                {1, GetService }
            };
        }

        public ulong Initialize(ServiceCallContext context)
        {
            IsInitialized = true;

            return 0;
        }

        public ulong GetService(ServiceCallContext context)
        {
            string name = context.Reader.ReadString(8);

            context.Response.HandleDescriptor = HandleDescriptor.MakeMove(new KSession(context.process, Factory.GetService(name), name).Handle);

            return 0;
        }
    }
}
