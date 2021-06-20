using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.lm
{
    public class ILogService : ICommand
    {
        public ILogService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Helper.GenerateCommandHandle<ILogger>() }
            };
        }
    }
}
