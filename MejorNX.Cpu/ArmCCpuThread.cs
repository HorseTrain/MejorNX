using MejorNX.Cpu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.Cpu
{
    unsafe struct threadContext
    {
        public fixed ulong X[32];
        public fixed ulong Q[64];

        ulong N;
        ulong Z;
        ulong C;
        ulong V;

        ulong dczid_el0;
        ulong fpcr;
        ulong fpsr;
        ulong tpidr;
        public ulong tpidrro_el0;

        public ulong svcHook;
        public ulong InstructionsExecuted;
        public ulong debugStepHook;

        //Free
        ulong Id;
        public ulong IsExecuting;
        ulong MemoryPointer;
        ulong MyPointer;
        ulong Return;

        ulong Open;
    };

    unsafe struct armProcess
    {
        public void* vmm;
    };

    public unsafe class ArmCCpuThread : CpuContext
    {
        [DllImport("ArmC.dll")]
        static extern void* armc_process_create(int pb, int ab);

        [DllImport("ArmC.dll")]
        static extern void* armc_process_thread_create(void* process);

        [DllImport("ArmC.dll")]
        static extern void* armc_vmm_map(void* ctx, ulong va, ulong size, void* ph);

        [DllImport("ArmC.dll")]
        static extern ulong armc_execute(void* process, void* thread, ulong entry, bool once);

        [DllImport("ArmC.dll")]
        static extern void armc_toggleDebugMode(bool deb);

        static bool open;

        static void* process;

        void* thread;

        ref threadContext ctx => ref *((threadContext*)thread);

        static Dictionary<ulong, ArmCCpuThread> ctxx = new Dictionary<ulong, ArmCCpuThread>();

        Hook svch;
        Hook dbhook;

        unsafe public ArmCCpuThread()
        {
            if (!open)
            {
                process = armc_process_create(12,32);

                armc_vmm_map(((armProcess*)process)->vmm,0,VirtualMemoryManager.RamSize,VirtualMemoryManager.BaseAddress);

                open = true;

                armc_toggleDebugMode(InDebugMode);
            }

            thread = armc_process_thread_create(process);

            ctxx.Add((ulong)thread, this);

            svch = new Hook(_CallSVC);
            dbhook = new Hook(_DebugHook);

            ctx.svcHook = (ulong)Marshal.GetFunctionPointerForDelegate(svch);
            ctx.debugStepHook = (ulong)Marshal.GetFunctionPointerForDelegate(dbhook);
        }

        protected override ulong GetX(ulong index) => ctx.X[index];
        protected override void SetX(ulong index, ulong Value) => ctx.X[index] = Value;

        public override ulong tpidrro_el0 { get => ctx.tpidrro_el0; set => ctx.tpidrro_el0 = value; }

        public override ulong PC { get; set; }
        public override ulong SP { get => GetX(31); set => SetX(31, value); }

        public override ulong ThreadID { get; set; }

        public static ArmCCpuThread CreateContext() => new ArmCCpuThread();

        public override void InitWorkFinished(WorkFinishedHLE command)
        {
        }

        public override void WriteIntToSharedAddress(ulong Position, int Value)
        {
            VirtualMemoryManager.GetWriter(Position).WriteStruct(Value);
        }

        public override void Execute()
        {
            armc_execute(process,thread,PC,false);
        }

        delegate void Hook(ulong context, ulong id);

        static void _CallSVC(ulong context, ulong id)
        {
            ctxx[context].CallSVC(ctxx[context],(int)id);
        }

        static StreamWriter writer = new StreamWriter(@"D:\Debug\GStep.txt");

        static void _DebugHook(ulong context, ulong address)
        {
            threadContext* ctx = (threadContext*)context;

            //Console.WriteLine(ctx->InstructionsExecuted);

            if (!InDebugMode)
                return;

            if (ctx->InstructionsExecuted >= DebugStart && ctx->InstructionsExecuted < DebugEnd)
            {
                writer.WriteLine($"{VirtualMemoryManager.ReverseBytes(VirtualMemoryManager.GetReader().ReadStruct<uint>(address)):x8} {ctx->InstructionsExecuted}");

                for (int i = 0; i < 32; i++)
                {
                    writer.WriteLine($"{i} {ctx->X[i]}");
                }
            }
            else if (ctx->InstructionsExecuted >= DebugEnd)
            {
                if (writer == null)
                    return;

                ctx->IsExecuting = 0;

                Console.WriteLine("Done");

                writer.Close();

                writer = null;
            }
        }
    }
}
