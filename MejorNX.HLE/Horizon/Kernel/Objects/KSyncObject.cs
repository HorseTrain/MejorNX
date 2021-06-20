using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KSyncObject : KObject
    {
        public ManualResetEvent HostEvent   { get; set; }

        public void Wait(ulong WaitTime) => HostEvent.WaitOne((int)WaitTime);
        public void Wait() => HostEvent.WaitOne();
        public void Send() => HostEvent.Set();

        public KSyncObject(Process process) : base(process)
        {
            HostEvent = new ManualResetEvent(false);

            Handle = process.SyncHandles.AddObject(this);
        }
    }
}
