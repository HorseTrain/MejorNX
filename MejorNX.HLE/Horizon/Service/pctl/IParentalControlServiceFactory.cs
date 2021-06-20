using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.pctl
{
    public class IParentalControlServiceFactory : ICommand
    {
        public IParentalControlServiceFactory()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Service.Helper.GenerateCommandHandle<IParentalControlService>() }
            };
        }
    }
}
