using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IDisplayController : ICommand
    {
        public IDisplayController()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
