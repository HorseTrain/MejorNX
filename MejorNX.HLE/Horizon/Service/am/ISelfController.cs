using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class ISelfController : ICommand
    {
        public ISelfController()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {13,    SetFocusHandlingMode},
                {16,    SetOutOfFocusSuspendingEnabled},
            };
        }

        ulong SetFocusHandlingMode(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong SetOutOfFocusSuspendingEnabled(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
