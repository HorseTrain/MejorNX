using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.pctl
{
    public class IParentalControlService : ICommand
    {
        public bool NeedsInit   { get; set; }
        bool Init               { get; set; }

        public IParentalControlService()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1, Initialize }
            };
        }

        public override void InitData(object obj)
        {
            NeedsInit = (bool)obj;
        }

        ulong Initialize(ServiceCallContext context)
        {
            if (NeedsInit && !Init)
            {
                Init = true;
            }          

            return 0;
        }
    }
}
