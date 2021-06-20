using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class Display
    {
        public uint Handle  { get; set; }
        public string Name  { get; set; }

        public Display(Process process, string name)
        {
            Handle = process.ServiceHandles.AddObject(this);
            Name = name;
        }
    }
}
