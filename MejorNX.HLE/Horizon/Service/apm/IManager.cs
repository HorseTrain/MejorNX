using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.apm
{
    public class IManager : ICommand
    {
        public IManager()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
