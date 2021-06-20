using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.ns
{
    public class IAddOnContentManager : ICommand
    {
        public IAddOnContentManager()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                { 2, CountAddOnContent },
                { 3, ListAddOnContent  }
            };
        }

        ulong CountAddOnContent(ServiceCallContext context)
        {
            context.Writer.WriteStruct(0);

            context.PrintStubbed();

            return 0;
        }

        public ulong ListAddOnContent(ServiceCallContext context)
        {
            context.PrintStubbed();

            context.Writer.WriteStruct(0);

            return 0;
        }
    }
}
