using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public class GpuDmaEngine : GpuEngine
    {
        public GpuDmaEngine(MaxwellContext context) : base(context)
        {
            Registers = new int[3584];
        }

        public void Call(GpuCommand command)
        {

        }
    }
}
