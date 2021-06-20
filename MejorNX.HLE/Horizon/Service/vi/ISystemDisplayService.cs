using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class ISystemDisplayService : ICommand
    {
        public ISystemDisplayService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                { 2205, SetLayerZ },
                { 2207, SetLayerVisibility },
                { 3200, GetDisplayMode }
            };
        }

        ulong SetLayerZ(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong SetLayerVisibility(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong GetDisplayMode(ServiceCallContext context)
        {
            context.Writer.WriteStruct(1280);
            context.Writer.WriteStruct(720);
            context.Writer.WriteStruct(60.0f);
            context.Writer.WriteStruct(0);
            return 0;
        }
    }
}
