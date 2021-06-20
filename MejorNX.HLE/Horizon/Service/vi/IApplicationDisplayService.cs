using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class IApplicationDisplayService : ICommand
    {
        public IApplicationDisplayService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {100,   GetRelayService },
                {101,   Helper.GenerateCommandHandle<ISystemDisplayService>() },
                {102,   Helper.GenerateCommandHandle<IManagerDisplayService>() },
                {1010,  OpenDisplay},
                {2030,  CreateStrayLayer},
                {2101,  SetLayerScalingMode},
            };
        }

        ulong OpenDisplay(ServiceCallContext context)
        {
            string name = context.Reader.ReadString();

            context.Writer.WriteStruct<ulong>(new Display(context.process, name).Handle);

            return 0;
        }

        ulong GetRelayService(ServiceCallContext context)
        {
            IHOSBinderDriver data = new IHOSBinderDriver(new KSyncObject(context.process));

            context.Data = data;

            Helper.Make(context);

            return 0;
        }

        ulong CreateStrayLayer(ServiceCallContext context)
        {
            ulong LayerFlags = context.Reader.ReadStruct<ulong>();
            ulong DisplayID = context.Reader.ReadStruct<ulong>();

            ulong ParcelPointer = context.Request.ReceiveDescriptors[0].Address;

            Display display = (Display)context.process.ServiceHandles.GetObject((uint)DisplayID);

            byte[] ParcelData = Parcel.MakeIGraphicsBufferProducer(ParcelPointer);

            VirtualMemoryManager.GetWriter(ParcelPointer).WriteStruct(ParcelData);

            context.Writer.WriteStruct(0L);
            context.Writer.WriteStruct((ulong)ParcelData.Length);

            return 0;
        }

        ulong SetLayerScalingMode(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
