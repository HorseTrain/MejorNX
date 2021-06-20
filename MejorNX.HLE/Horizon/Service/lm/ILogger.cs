using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.lm
{
    public class ILogger : ICommand
    {
        public ILogger()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Log }
            };
        }

        ulong Log(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
