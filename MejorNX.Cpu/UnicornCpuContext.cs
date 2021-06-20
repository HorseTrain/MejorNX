using MejorNX.Cpu.Memory;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnicornNET;

namespace MejorNX.Cpu
{
    public unsafe class UnicornCpuContext : CpuContext
    {
        Arm64Engine engine { get; set; }

        ulong uThreadID { get; set; }

        public UnicornCpuContext()
        {
            engine = new Arm64Engine();

            engine.MapMemory(VirtualMemoryManager.RamSize, VirtualMemoryManager.BaseAddress);

            engine.AddHookSVC(UnicornSVCHook);
        }

        protected override ulong GetX(ulong index) =>               engine.GetX((int)index);
        protected override void SetX(ulong index, ulong Value) =>   engine.SetX((int)index,Value);

        public override ulong tpidrro_el0 { get => engine.GetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_TPIDRRO_EL0); set => engine.SetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_TPIDRRO_EL0, value); }

        public override ulong PC { get => engine.GetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_PC); set => engine.SetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_PC, value); }
        public override ulong SP { get => engine.GetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_SP); set => engine.SetRegRaw((int)uc_arm64_reg.UC_ARM64_REG_SP, value); }

        public override ulong ThreadID { get => uThreadID; set => this.uThreadID = value; }

        StreamWriter writer;

        public override void Execute()
        {
            if (InDebugMode)
            {
                writer = new StreamWriter(@"D:\Debug\UStep.txt");

                if (DebugStart != 0)
                engine.Step(DebugStart);

                for (ulong i = DebugStart; i < DebugEnd; i++)
                {
                    writer.WriteLine($"{VirtualMemoryManager.ReverseBytes(VirtualMemoryManager.GetReader().ReadStruct<uint>(PC)):x8} {i}");

                    engine.Step(1);

                    for (int r = 0; r < 32; r++)
                    {
                        writer.WriteLine($"{r} {GetX((ulong)r)}");
                    }
                }

                writer.Close();

                Console.WriteLine("Done");
            }
            else
            {
                engine.Step(0);
            }
        }


        public void UnicornSVCHook()
        {
            MemoryReader reader = VirtualMemoryManager.GetReader();

            reader.Seek(PC - 4);

            int id = (reader.ReadStruct<int>() >> 5) & 0x7FFF;

            CallSVC(this,id);
        }

        public static UnicornCpuContext CreateContext() => new UnicornCpuContext();

        public override void InitWorkFinished(WorkFinishedHLE command)
        {
        }

        public override void WriteIntToSharedAddress(ulong Position, int Value)
        {
            VirtualMemoryManager.GetWriter(Position).WriteStruct(Value);
        }
    }
}
