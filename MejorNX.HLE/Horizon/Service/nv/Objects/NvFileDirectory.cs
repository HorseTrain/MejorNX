using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv.Objects
{
    public class NvFileDirectory
    {
        public string Name { get; set; }

        public NvFileDirectory(string Name)
        {
            this.Name = Name;
        }
    }
}
