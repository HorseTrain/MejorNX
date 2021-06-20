using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.acc
{
    public class IProfile : ICommand
    {
        public IProfile()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1, GetBase }
            };
        }

        ulong GetBase(ServiceCallContext context)
        {
            context.Writer.WriteStruct(0L);

            context.PrintStubbed();

            return 0;
        }
    }
}
