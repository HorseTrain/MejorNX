using MejorNX.Common.Utilities;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Service.am;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon
{
    public class HorizonOS
    {
        public static HorizonOS MainOS          { get; set; }

        public List<Process> Processes          { get; set; }
        public Schedular schedular              { get; set; }
        public KSharedMemory HidSharedMemory    { get; set; }

        public HorizonOS()
        {
            MainOS = this;

            Processes = new List<Process>();
            schedular = new Schedular();

            HidSharedMemory = new KSharedMemory();
        }

        public Process OpenProcess()
        {
            Process Out = new Process();

            Processes.Add(Out);

            return Out;
        }
    }
}
