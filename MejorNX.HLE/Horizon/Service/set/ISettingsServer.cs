using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.set
{
    public class ISettingsServer : ICommand
    {
        public ISettingsServer()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1, GetAvailableLanguageCodes }
            };
        }

        ulong GetAvailableLanguageCodes(ServiceCallContext context)
        {
            context.Writer.WriteStruct(15);

            context.PrintStubbed();

            return 0;
        }
    }
}
