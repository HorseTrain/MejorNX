using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.Cpu
{
    public unsafe class GalacticARMCpuContext : CpuContext
    {
        static bool isOpen = false;

        CpuThread thread;

        WorkFinishedHLE command;

        public GalacticARMCpuContext()
        {
            thread = CpuThread.CreateThread();

            if (!isOpen)
            {
                isOpen = true;

                VirtualMemoryManager.MapMemory(0,Memory.VirtualMemoryManager.BaseAddress, Memory.VirtualMemoryManager.RamSize);
            }

            thread.svc = _svc;
        }

        public static GalacticARMCpuContext CreateContext() => new GalacticARMCpuContext();

        public override void InitWorkFinished(WorkFinishedHLE command)
        {
            this.command = command;
        }

        protected override ulong GetX(ulong index)
        {
            return thread.Context.GetX((int)index);
        }

        protected override void SetX(ulong index, ulong Value)
        {
            thread.Context.SetX((int)index,Value);
        }

        public override void Execute()
        {
            thread.Execute(PC);

            command();
        }

        void _svc(int id)
        {
            CallSVC(this,id);
        }

        public override ulong PC { get; set; }
        public override ulong SP { get => thread.Context.X31; set => thread.Context.X31 = value; }

        public override ulong tpidrro_el0 { get => thread.Context.tpidrro_el0; set => thread.Context.tpidrro_el0 = value; }
    }
}
