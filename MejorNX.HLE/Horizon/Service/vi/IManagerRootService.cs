using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class IManagerRootService : ICommand
    {
        public IManagerRootService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {2, Helper.GenerateCommandHandle<IApplicationDisplayService>() }
            };
        }
    }
}
