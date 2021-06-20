using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public delegate int IoctlProcessor(ServiceCallContext context, int Cmd);
}
