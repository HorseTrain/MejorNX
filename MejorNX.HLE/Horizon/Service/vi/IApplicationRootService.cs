using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class IApplicationRootService : ICommand
    {
        public IApplicationRootService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {2, Helper.GenerateCommandHandle<IApplicationDisplayService>() }
            };
        }
    }
}
