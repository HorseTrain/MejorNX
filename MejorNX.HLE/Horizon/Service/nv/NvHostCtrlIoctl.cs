using MejorNX.Common.Debugging;
using MejorNX.Common.Utilities;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Service.nv.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class NvHostCtrlIoctl 
    {
        static ObjectCollection UserContexts { get; set; } = new ObjectCollection();

        public static int ProcessIoctl(ServiceCallContext context, int Command)
        {
            switch (Command & 0xffff)
            {
                case 0x1e: return 0;
                case 0x1f: return EventRegister(context);
            }
            Debug.LogError($"Unknown Command 0x{(Command & 0xffff).ToString("X")}", true);

            return 0;
        }

        static int SyncptRead(ServiceCallContext context)
        {
            SyncptReadMinOrMax(context, false);

            return 0;
        }

        static int EventRegister(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int SyncptReadMinOrMax(ServiceCallContext context, bool Max)
        {
            throw new NotImplementedException();

            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvHostCtrlSyncptRead Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvHostCtrlSyncptRead>();

            if (Max)
            {

            }
            else
            {

            }
        }

    }
}
