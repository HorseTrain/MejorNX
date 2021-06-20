using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public class IManagerDisplayService : ICommand
    {
        public IManagerDisplayService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                { 2010, CreateManagedLayer  },
                { 2011, DestroyManagedLayer },
                { 6000, AddToLayerStack     },
                { 6002, SetLayerVisibility  }
            };
        }

        ulong CreateManagedLayer(ServiceCallContext context)
        {
            context.PrintStubbed();

            context.Writer.WriteStruct(0L);

            return 0;
        }

        ulong DestroyManagedLayer(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong AddToLayerStack(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong SetLayerVisibility(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
