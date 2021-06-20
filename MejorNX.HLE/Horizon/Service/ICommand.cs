using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service
{
    public class ICommand
    {
        public virtual Dictionary<ulong, ServiceCall> Calls     { get; set; }

        public virtual void InitData(object obj)
        {
            //Console.WriteLine(GetType());

            throw new NotImplementedException();
        }
    }
}
