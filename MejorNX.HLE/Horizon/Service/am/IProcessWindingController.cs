using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IProcessWindingController : ICommand
    {
        public IProcessWindingController()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
