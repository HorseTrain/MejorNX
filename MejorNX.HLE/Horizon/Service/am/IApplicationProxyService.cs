using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IApplicationProxyService : ICommand
    {
        public IApplicationProxyService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Service.Helper.GenerateCommandHandle<IApplicationProxy>() }
            };
        }


    }
}
