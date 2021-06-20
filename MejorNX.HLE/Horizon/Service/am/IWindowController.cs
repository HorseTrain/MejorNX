using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IWindowController : ICommand
    {
        public IWindowController()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1,     GetAppletResourceUserId},
                {10,    AcquireForegroundRights},
            };
        }

        ulong GetAppletResourceUserId(ServiceCallContext context)
        {
            context.PrintStubbed();

            context.Writer.WriteStruct(0L);

            return 0;
        }

        ulong AcquireForegroundRights(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
