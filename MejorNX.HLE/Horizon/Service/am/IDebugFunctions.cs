using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IDebugFunctions : ICommand
    {
        public IDebugFunctions()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
