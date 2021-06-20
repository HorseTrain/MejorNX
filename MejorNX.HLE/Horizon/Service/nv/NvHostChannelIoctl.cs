using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Service.nv.Structs;
using MejorNX.Maxwell;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class NvHostChannelIoctl
    {
        public static int ProcessIoctl(ServiceCallContext context, int Command)
        {
            switch (Command & 0xffff)
            {
                case 0x4714: return SetUserData(context);
                case 0x4801: return SetNvMap(context);
                case 0x4808: return SubmitGpfifo(context);
                case 0x4809: return AllocObjCtx(context);
                case 0x480b: return ZcullBind(context);
                case 0x480c: return SetErrorNotifier(context);
                case 0x480d: return SetPriority(context);
                case 0x481a: return AllocGpfifoEx2(context);
            }

            Debug.LogError($"0x{(Command & 0xffff):x} Does not exist.",true);

            return 0;
        }

        static int SetNvMap(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int SubmitGpfifo(ServiceCallContext context)
        {
            ulong InputPosition = context.Request.GetBufferType0x21().Position;
            ulong OutputPosition = context.Request.GetBufferType0x22().Position;

            MemoryReader reader = VirtualMemoryManager.GetReader(InputPosition);

            NvHostChannelSubmitGpfifo Args = reader.ReadStruct<NvHostChannelSubmitGpfifo>();

            for (int i = 0; i < Args.NumEntries; i++)
            {
                ulong Gpfifo = reader.ReadStructAtOffset<ulong>((ulong)((long)InputPosition + 0x18 + i * 8));

                ulong VA = Gpfifo & 0xff_ffff_ffff;

                int Size = (int)(Gpfifo >> 40) & 0x7ffffc;

                ulong PA = MaxwellContext.MainContext.Vmm.GetPhysicalAddress(VA);

                reader.Seek(PA);

                Switch.MainSwitch.Gpu.CommandStack.PushCommandCollection(Decode(reader, PA + (ulong)Size));
            }

            Args.SyncptId = 0;
            Args.SyncptValue = 0;

            VirtualMemoryManager.GetWriter(OutputPosition).WriteStruct(Args);

            return 0;
        }

        static int AllocGpfifoEx2(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int AllocObjCtx(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int SetErrorNotifier(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int SetUserData(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int SetPriority(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        static int ZcullBind(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        enum SubmissionMode
        {
            Incrementing = 1,
            NonIncrementing = 3,
            Immediate = 4,
            IncrementOnce = 5
        }

        public static GpuCommand[] Decode(MemoryReader reader, ulong Top)
        {
            List<GpuCommand> Out = new List<GpuCommand>();

            bool CanRead() => reader.Location + 4 < Top;

            int i = 0;

            while (CanRead())
            {
                int Word0 = reader.ReadStruct<int>();

                //Console.WriteLine(i + " " + Word0);

                i++;

                int Method = (Word0 >> 0) & 0x1fff;
                int SubC = (Word0 >> 13) & 7;
                int Args = (Word0 >> 16) & 0x1fff;
                SubmissionMode Mode = (SubmissionMode)((Word0 >> 29) & 7);

                switch (Mode)
                {
                    case SubmissionMode.Incrementing:
                    {

                        for (int Index = 0; Index < Args; Index++, Method++)
                        {
                            Out.Add(new GpuCommand(Method, SubC, reader.ReadStruct<int>()));
                        }

                        break;
                    }

                    case SubmissionMode.NonIncrementing:
                    {

                        int[] Arguments = new int[Args];

                        for (int Index = 0; Index < Arguments.Length; Index++)
                        {
                            if (!CanRead())
                            {
                                break;
                            }

                            Arguments[Index] = reader.ReadStruct<int>();
                        }

                        Out.Add(new GpuCommand(Method, SubC, Arguments));

                        break;
                    }

                    case SubmissionMode.Immediate: Out.Add(new GpuCommand(Method, SubC, Args)); break;

                    case SubmissionMode.IncrementOnce:
                    {

                        if (CanRead())
                        {
                            Out.Add(new GpuCommand(Method, SubC, reader.ReadStruct<int>()));
                        }

                        if (CanRead() && Args > 1)
                        {
                            int[] Arguments = new int[Args - 1];

                            for (int Index = 0; Index < Arguments.Length && CanRead(); Index++)
                            {
                                Arguments[Index] = reader.ReadStruct<int>();
                            }

                            Out.Add(new GpuCommand(Method + 1, SubC, Arguments));
                        }

                        break;
                    }
                }
            }

            return Out.ToArray();
        }
    }
}
