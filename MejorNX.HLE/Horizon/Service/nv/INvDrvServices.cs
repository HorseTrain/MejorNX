using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Service.nv.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class INvDrvServices : ICommand
    {
        public Dictionary<string,IoctlProcessor> IoctlCommands      { get; set; }

        KSyncObject Event;

        public INvDrvServices()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Open },
                {1, Ioctl },
                {3, Initialize },
                {4, QueryEvent }
            };

            IoctlCommands = new Dictionary<string, IoctlProcessor>()
            {
                { "/dev/nvhost-gpu",        ProcessIoctlNvHostChannel   },
                { "/dev/nvhost-as-gpu",     ProcessIoctlNvGpuAS         },
                { "/dev/nvhost-ctrl-gpu",   ProcessIoctlNvGpuGpu        },
                { "/dev/nvmap",             ProcessIoctlNvMap           },
                { "/dev/nvhost-ctrl",       ProcessIoctlNvHostCtrl      },
            };
        }

        ulong Open(ServiceCallContext context)
        {
            ulong NamePointer = context.Request.SendDescriptors[0].Address;

            string name = VirtualMemoryManager.GetReader(NamePointer).ReadString();

            context.Writer.WriteStruct(context.process.ServiceHandles.AddObject(new NvFileDirectory(name)));
            context.Writer.WriteStruct(0);

            return 0;
        }

        ulong Ioctl(ServiceCallContext context)
        {
            uint Fd = context.Reader.ReadStruct<uint>();
            uint Command = context.Reader.ReadStruct<uint>();

            NvFileDirectory fd = (NvFileDirectory)context.process.ServiceHandles.GetObject(Fd);

            IoctlProcessor Process;

            int Result;

            if (IoctlCommands.TryGetValue(fd.Name,out Process))
            {
                Result = Process(context,(int)Command);
            }
            else
            {
                Debug.LogError($"Service {fd.Name} {Command} Does not exist.",true);

                return 0;
            }

            context.Writer.WriteStruct(Result);

            return 0;
        }

        ulong Initialize(ServiceCallContext context)
        {
            //NvMap map = new NvMap();

            //context.process.ServiceHandles.AddObject(map);

            //You'd expect this to have some sort of writeback ?

            //context.Writer.WriteStruct(0);

            context.PrintStubbed();

            return 0;
        }

        ulong QueryEvent(ServiceCallContext context)
        {
            if (Event == null)
            {
                Event = new KSyncObject(context.process);
            }

            //In ryujinx, a handle is opened right here.
            context.Response.HandleDescriptor = HandleDescriptor.MakeCopy(context.process.SyncHandles.AddObject(Event));

            context.Writer.WriteStruct(0);

            return 0;
        }

        int ProcessIoctlNvHostChannel(ServiceCallContext context, int Command) => ProcessIoctl(context,Command,NvHostChannelIoctl.ProcessIoctl);
        int ProcessIoctlNvGpuAS(ServiceCallContext context, int Command) => ProcessIoctl(context,Command,NvGpuASIoctl.ProcessIoctl);
        int ProcessIoctlNvGpuGpu(ServiceCallContext context, int Command) => ProcessIoctl(context, Command, NvGpuGpuIoctl.ProcessIoctl);
        int ProcessIoctlNvMap(ServiceCallContext context, int Command) => ProcessIoctl(context,Command,NvMapIoctl.ProcessIoctl);
        int ProcessIoctlNvHostCtrl(ServiceCallContext context, int Command) => ProcessIoctl(context,Command, NvHostCtrlIoctl.ProcessIoctl);

        int ProcessIoctl(ServiceCallContext context, int Cmd, IoctlProcessor Processor)
        {
            //TODO: Error commands

            return Processor(context,Cmd);
        }

    }
}
