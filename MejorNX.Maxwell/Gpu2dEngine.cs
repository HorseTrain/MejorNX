using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public class Gpu2dEngine : GpuEngine
    {
        public Gpu2dEngine(MaxwellContext context) : base (context)
        {
            Registers = new int[0xe00];
        }

        public void Call(GpuCommand command) => CallMethod(command);
    }
}
