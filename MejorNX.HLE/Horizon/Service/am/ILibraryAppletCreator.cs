using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class ILibraryAppletCreator : ICommand
    {
        public ILibraryAppletCreator()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }
    }
}
