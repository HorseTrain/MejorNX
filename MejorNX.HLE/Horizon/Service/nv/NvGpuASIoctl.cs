using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Service.nv.Objects;
using MejorNX.HLE.Horizon.Service.nv.Structs;
using MejorNX.Maxwell;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class NvGpuASIoctl
    {
        public static int ProcessIoctl(ServiceCallContext context, int Command)
        {
            switch (Command & 0xffff)
            {
                case 0x4101: return BindChannel(context);
                case 0x4102: return AllocSpace(context);
                case 0x4106: return MapBufferEx(context);
                case 0x4108: return GetVaRegions(context);
                case 0x4109: return InitializeEx(context);
            }

            Debug.LogError($"Unknown Command 0x{(Command & 0xffff).ToString("X")}", true);

            return 0;
        }

        static int BindChannel(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int AllocSpace(ServiceCallContext context)
        {
            //TODO: Error Codes.

            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASAllocSpace Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvGpuASAllocSpace>();

            MaxwellVirtualMemoryManager GpuVMM = Switch.MainSwitch.Gpu.Vmm;

            //Unsigned ?
            ulong Size = (ulong)Args.Pages * (ulong)Args.PageSize;

            if ((Args.Flags & 1) != 0)
            {
                throw new NotImplementedException();
            }
            else
            {
                Args.Offset = 0;

                context.PrintStubbed();
            }

            if (Args.Offset < 0)
            {
                throw new Exception();
            }

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);
            
            return 0;
        }

        static ulong[] stub = new ulong[]
        {
            4294967296,
4294971392,
4294975488,
4295041024,
4295061504,
4295081984,
4295086080,
4295090176,
4295094272,
4295098368,
4295102464,
4295106560,
4303953920,
4312801280,
4321648640,
4330496000,
4339343360,
4348190720,
4357038080,
4365885440,
4366147584,
4366409728,
4366671872,
4366934016,
4367196160,
4367458304,
4367720448,
4367982592,
4370079744,
4372176896,
4374274048,
4376371200,
4376436736,
4376502272,
4376801280,
4377100288,
4378148864,
4379197440,
4379201536,
4379205632,
4379209728,
4379217920,
4379222016,
4379226112,
4379230208,
        };

        static int s = 0;

        static int MapBufferEx(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuASMapBufferEx Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvGpuASMapBufferEx>();

            NvMap Map = NvMap.GetMap((uint)Args.NvMapHandle);

            ulong PA = Map.Address + Args.BufferOffset;

            ulong Size = Args.MappingSize;

            if (Size == 0)
            {
                Size = Map.Size;
            }

            int result = 0;

            if ((Args.Flags & 1) != 0)
            {
                //TODO: Error codes.

                MaxwellContext.MainContext.Vmm.MapMemory(Args.Offset,Size,PA);
            }
            else
            {
                Args.Offset = stub[s];

                MaxwellContext.MainContext.Vmm.MapMemory(Args.Offset, Size, PA);

                //Args.Offset = MaxwellContext.MainContext.Vmm.Map(Size,PA);

                //

                s++;
            }

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return result;
        }

        static int GetVaRegions(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int InitializeEx(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
