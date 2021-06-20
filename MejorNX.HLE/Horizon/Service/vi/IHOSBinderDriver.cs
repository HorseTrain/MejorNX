using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class IHOSBinderDriver : ICommand
    {
        public KSyncObject ReleaseEvent;
        public NvFlinger Flinger;

        public IHOSBinderDriver(KSyncObject ReleaseEvent)
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, TransactParcel },
                {1, AdjustRefcount},
                {2, GetNativeHandle }
            };

            this.ReleaseEvent = ReleaseEvent;

            Flinger = new NvFlinger(Switch.MainSwitch.Gpu,ReleaseEvent);
        }

        ulong TransactParcel(ServiceCallContext context)
        {
            context.Reader.Advance(4); //Id
            int Command = context.Reader.ReadStruct<int>();

            ulong DataPosition = context.Request.SendDescriptors[0].Address;
            ulong DataSize = context.Request.SendDescriptors[0].Size;

            MemoryReader reader = VirtualMemoryManager.GetReader(DataPosition);

            return Flinger.ProcessParcelRequest(context, Parcel.GetParcelData(reader.ReadArray<byte>(DataSize)), Command);
        }

        ulong AdjustRefcount(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong GetNativeHandle(ServiceCallContext context)
        {
            context.Response.HandleDescriptor = HandleDescriptor.MakeMove(ReleaseEvent.Handle);

            return 0;
        }
    }
}
