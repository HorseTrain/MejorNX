using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Kernel.SVC;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MejorNX.HLE.Horizon.Service
{
    public class ServiceCallContext
    {
        public KSession Session     { get; set; }
        public IPCCommand Request   { get; set; }
        public IPCCommand Response  { get; set; }
        public MemoryReader Reader  { get; set; }
        public BinaryBuilder Writer { get; set; }

        public ulong CommandPointer { get; set; }
        public Process process      { get; set; }
        public ServiceCall Service  { get; set; }
        public ulong CommandID      { get; set; }
        public bool Ignore          { get; set; }

        public object Data          { get; set; }

        public void PrintStubbed()
        {
            Debug.LogWarning($"Service {Service.Method.Name} Stubbed");
        }

        public void LogUnknown()
        {
            Debug.LogError($"Unknown Service: {Session.Name} {CommandID}");
        }

        public ulong CallService()
        {
            //Debug.Log($"Service {Service.Method.Name} Called " + SvcCollection.index);

            return Service(this);
        }
    }
}
