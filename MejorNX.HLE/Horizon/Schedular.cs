using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Kernel.SVC;
using static MejorNX.Cpu.Memory.VirtualMemoryManager;
using static MejorNX.HLE.Horizon.HorizonOS;
using static MejorNX.HLE.Horizon.Kernel.Objects.KObject;
using System.Collections.Generic;
using System.Threading;
using System;
using MejorNX.Common.Debugging;

namespace MejorNX.HLE.Horizon
{
    public class Schedular
    {
        static ulong TLS                        { get; set; }
        static ulong OpenThreads                { get; set; } = 1;

        public static Schedular MainSched       { get; set; }

        public List<KThread> Threads            { get; set; }
        public List<KThread> SuspendedThreads   { get; set; }
        public List<KThread> ThreadArbiterList  { get; set; }
        public object ThreadSyncLock            { get; set; }

        public Schedular()
        {
            MainSched = this;

            Threads = new List<KThread>();
            ThreadArbiterList = new List<KThread>();
            SuspendedThreads = new List<KThread>();
            ThreadSyncLock = new object();

            TLS = TlsCollectionAddress;
        }

        public KThread MakeThread(Process process,ulong PC, ulong SP, ulong Arguments, ulong Priority, int ProcessorId)
        {
            KThread Out = new KThread(process);

            //Out.Cpu.ThreadID = OpenThreads;

            Out.Cpu.PC = PC;
            Out.Cpu.SP = SP;
            Out.Cpu.tpidrro_el0 = TLS;

            TLS += 0x200;
            OpenThreads++;

            Out.Cpu.X[0] = Arguments;
            Out.Cpu.X[1] = Out.Handle;

            Out.ThreadPriority = Priority;
            Out.ProcessorId = ProcessorId;

            Threads.Add(Out);

            Out.Cpu.CallSVC = SvcCollection.Call;

            return Out;
        }

        public void DetatchAndExecuteKThread(KThread thread)
        {
            thread.HostThread.Start();
        }

        public static KThread GetThread(object Handle) => (KThread)Handle;

        public static T GetObject<T>(object Context, uint Handle)
        {
            return (T)GetThread(Context).Process.ServiceHandles[Handle];
        }
    }
}
