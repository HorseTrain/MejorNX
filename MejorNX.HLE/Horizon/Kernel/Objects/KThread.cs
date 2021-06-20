using MejorNX.Cpu;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KThread : KSyncObject
    {
        public CpuContext Cpu           { get; set; }
        public Thread HostThread        { get; set; }

        public ulong ThreadPriority     { get; set; }
        public uint ID                  { get; set; }
        static uint Count               { get; set; }

        public ulong WaitHandle         { get; set; }
        public ulong MutexAddress       { get; set; }
        public bool CondVarSignaled     { get; set; }
        public ulong CondVarAddress     { get; set; }
        public KThread MutexOwner       { get; set; }
        public List<KThread> MutexWaiters   { get; set; }

        public int ProcessorId          { get; set; }

        public KThread(Process process) : base(process)
        {
            Cpu = CpuContext.GenerateCPU();
            Cpu.ThreadInformation = this;

            HostThread = new Thread(Execute);

            MutexWaiters = new List<KThread>();

            ID = Count;
            Count++;

            //Handle = process.ServiceHandles.AddObject(this);

            Cpu.InitWorkFinished(WorkFinished);
        }

        public void Execute()
        {
            Cpu.Execute();
        }

        public void WorkFinished()
        {
            Send();
        }
    }
}
