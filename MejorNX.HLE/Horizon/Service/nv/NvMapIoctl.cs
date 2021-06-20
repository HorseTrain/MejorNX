using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Service.nv.Objects;
using MejorNX.HLE.Horizon.Service.nv.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class NvMapIoctl
    {
        public static int RoundUp(int Value, int Size)
        {
            return (Value + (Size - 1)) & ~(Size - 1);
        }

        public static int ProcessIoctl(ServiceCallContext context, int Command)
        {
            switch (Command & 0xffff)
            {
                case 0x0101: return Create(context);
                case 0x0104: return Alloc(context);
                case 0x010e: return GetId(context);
            }

            Debug.LogError($"Map Command 0x{(Command & 0xffff):x} Unsupported",true);

            return -25;
        }

        static int Create(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvMapCreate Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvMapCreate>();

            if (Args.Size == 0)
            {
                throw new Exception();
            }

            uint Size = (uint)RoundUp(Args.Size, 4096);

            NvMap map = new NvMap(Size);

            Debug.Log($"Created NvMap {map.Handle} with size: {Size:x8}");

            Args.Handle = (int)map.Handle;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int Alloc(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvMapAlloc Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvMapAlloc>();

            NvMap Map = NvMap.GetMap((uint)Args.Handle);

            if (!NvMap.Handles.ContainsObject(0))
            {
                NvMap.Handles.Objects.Add(0,new NvMap());
            }

            if ((Args.Align & (Args.Align - 1)) != 0)
            {
                throw new Exception();
            }

            if ((uint)Args.Align < 4096)
            {
                Args.Align = 4096;
            }

            int Result = 0;

            if (!Map.Allocated)
            {
                Map.Allocated = true;

                Map.Aling = (uint)Args.Align;
                Map.Kind = (byte)Args.Kind;

                uint size = (uint)RoundUp((int)Map.Size,4096);

                ulong Address = Args.Address;

                if (Address == 0)
                {
                    throw new Exception();
                }

                if (Result == 0)
                {
                    Map.Size = size;
                    Map.Address = Address;
                }
            }

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return Result;
        }

        static int GetId(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvMapGetId Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvMapGetId>();

            NvMap Map = NvMap.GetMap((uint)Args.Handle);

            Args.Id = (int)Map.Handle;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }
    }
}
