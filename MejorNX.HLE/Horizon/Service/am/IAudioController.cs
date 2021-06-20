using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IAudioController : ICommand
    {
        public IAudioController()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
