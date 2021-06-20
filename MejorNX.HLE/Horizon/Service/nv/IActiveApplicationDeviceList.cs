using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.nv
{
    public class IActiveApplicationDeviceList : ICommand
    {
        public IActiveApplicationDeviceList()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, ActivateVibrationDevice }
            };
        }

        ulong ActivateVibrationDevice(ServiceCallContext context)
        {
            return 0;
        }
    }
}
