using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Service.nv.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    //NOTE: The structs were ripped directly from ryujinx.

    public class NvGpuGpuIoctl
    {
        public static int ProcessIoctl(ServiceCallContext context, int Command)
        {
            switch (Command & 0xffff)
            {
                case 0x4701: return ZcullGetCtxSize(context);
                case 0x4702: return ZcullGetInfo(context);
                case 0x4705: return GetCharacteristics(context);
                case 0x4706: return GetTpcMasks(context);
                case 0x4714: return GetActiveSlotMask(context);
            }

            Debug.LogError($"Unknown Command 0x{(Command & 0xffff).ToString("X")}",true);

            return 0;
        }

        static int ZcullGetCtxSize(ServiceCallContext context)
        {
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetCtxSize Args = new NvGpuGpuZcullGetCtxSize();

            Args.Size = 1;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int ZcullGetInfo(ServiceCallContext context)
        {
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuZcullGetInfo Args = new NvGpuGpuZcullGetInfo();

            Args.WidthAlignPixels = 0x20;
            Args.HeightAlignPixels = 0x20;
            Args.PixelSquaresByAliquots = 0x400;
            Args.AliquotTotal = 0x800;
            Args.RegionByteMultiplier = 0x20;
            Args.RegionHeaderSize = 0x20;
            Args.SubregionHeaderSize = 0xc0;
            Args.SubregionWidthAlignPixels = 0x20;
            Args.SubregionHeightAlignPixels = 0x40;
            Args.SubregionCount = 0x10;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int GetCharacteristics(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            MemoryReader reader = VirtualMemoryManager.GetReader(InputPosition);

            //NvGpuGpuGetCharacteristics Args = reader.ReadStruct<NvGpuGpuGetCharacteristics>(); Do i really need to read?

            NvGpuGpuGetCharacteristics Args = new NvGpuGpuGetCharacteristics();

            Args.BufferSize = 0xa0;
            Args.Arch = 0x120;
            Args.Impl = 0xb;
            Args.Rev = 0xa1;
            Args.NumGpc = 0x1;
            Args.L2CacheSize = 0x40000;
            Args.OnBoardVideoMemorySize = 0x0;
            Args.NumTpcPerGpc = 0x2;
            Args.BusType = 0x20;
            Args.BigPageSize = 0x20000;
            Args.CompressionPageSize = 0x20000;
            Args.PdeCoverageBitCount = 0x1b;
            Args.AvailableBigPageSizes = 0x30000;
            Args.GpcMask = 0x1;
            Args.SmArchSmVersion = 0x503;
            Args.SmArchSpaVersion = 0x503;
            Args.SmArchWarpCount = 0x80;
            Args.GpuVaBitCount = 0x28;
            Args.Reserved = 0x0;
            Args.Flags = 0x55;
            Args.TwodClass = 0x902d;
            Args.ThreedClass = 0xb197;
            Args.ComputeClass = 0xb1c0;
            Args.GpfifoClass = 0xb06f;
            Args.InlineToMemoryClass = 0xa140;
            Args.DmaCopyClass = 0xb0b5;
            Args.MaxFbpsCount = 0x1;
            Args.FbpEnMask = 0x0;
            Args.MaxLtcPerFbp = 0x2;
            Args.MaxLtsPerLtc = 0x1;
            Args.MaxTexPerTpc = 0x0;
            Args.MaxGpcCount = 0x1;
            Args.RopL2EnMask0 = 0x21d70;
            Args.RopL2EnMask1 = 0x0;
            Args.ChipName = 0x6230326d67;
            Args.GrCompbitStoreBaseHw = 0x0;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int GetTpcMasks(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetTpcMasks Args = VirtualMemoryManager.GetReader(InputPosition).ReadStruct<NvGpuGpuGetTpcMasks>();

            if (Args.MaskBufferSize != 0)
            {
                Args.TpcMask = 3;
            }

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int GetActiveSlotMask(ServiceCallContext context)
        {
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            NvGpuGpuGetActiveSlotMask Args = new NvGpuGpuGetActiveSlotMask();

            Args.Slot = 0x07;
            Args.Mask = 0x01;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            context.PrintStubbed();

            return 0;
        }
    }
}
