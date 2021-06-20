using MejorNX.Cpu.Memory;
using MejorNX.Cpu.Utilities.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu
{
    public delegate CpuContext CreateThread();

    public delegate void SvcCall(CpuContext context, int svc);
    public delegate void WorkFinishedHLE();

    public class CpuContext
    {
        public static bool InDebugMode = true;
        public static ulong DebugStart = 0;
        public static ulong DebugEnd = 4;

        public static CreateThread GenerateCPU = InitNull;

        public object ThreadInformation { get; set; }

        public static VirtualMemoryManager MemoryManager { get; set; }

        public ObjectIndexer<ulong> X => new ObjectIndexer<ulong>(GetX, SetX, this);
        public ObjectIndexer<uint> W => new ObjectIndexer<uint>(GetW, SetW, this);

        protected virtual ulong GetX(ulong index)
        {
            throw new NotImplementedException();
        }

        protected virtual void SetX(ulong index, ulong Value)
        {
            throw new NotImplementedException();
        }

        uint GetW(ulong index) => (uint)GetX(index);
        void SetW(ulong index, uint value) => SetX(index, value);

        public virtual ulong ProcessId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ulong ThreadID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual ulong tpidrro_el0 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ulong dczid_el0 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual ulong PC { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ulong SP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual SvcCall CallSVC { get; set; } = null;

        public virtual void Execute()
        {
            throw new NotImplementedException();
        }

        public static void InitCPU()
        {
            MemoryManager = new VirtualMemoryManager();
        }

        public static CpuContext InitNull() => new CpuContext();

        public virtual void StopExecution() => throw new NotImplementedException();

        public virtual void InitWorkFinished(WorkFinishedHLE command) => throw new NotImplementedException();

        //Monitors
        public virtual bool TestExclusive(ulong Position)
        {
            return true;
        }
        public virtual void ClearExclusive()
        {

        }
        public virtual void SetExclusive(ulong Position)
        {

        }
        public unsafe virtual void WriteIntToSharedAddress(ulong Position, int Value)
        {
            *((int*)(VirtualMemoryManager.BaseAddress + Position)) = Value;
        }
    }
}
