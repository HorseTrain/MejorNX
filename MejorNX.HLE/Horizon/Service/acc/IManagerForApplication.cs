using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.acc
{
    public class IManagerForApplication : ICommand
    {
        public IManagerForApplication()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, CheckAvailability},
                {1, GetAccountId    },
            };
        }

        ulong CheckAvailability(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong GetAccountId(ServiceCallContext context)
        {
            context.PrintStubbed();

            context.Writer.WriteStruct(0xcafeL);

            return 0;
        }
    }
}
