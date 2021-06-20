using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.ssl
{
    public class ISslService : ICommand
    {
        public ISslService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, SetInterfaceVersion }
            };
        }

        ulong SetInterfaceVersion(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
