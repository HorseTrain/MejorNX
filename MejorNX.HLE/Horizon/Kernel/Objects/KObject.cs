using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KObject
    {
        public uint Handle      { get; set; }
        public Process Process  { get; set; }
        public string Name      { get; set; }

        public KObject(Process process, string Name = "")
        {
            Process = process;
            this.Name = Name;
        }
    }
}
