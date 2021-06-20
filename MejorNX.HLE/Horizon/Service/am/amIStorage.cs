using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class amIStorage : ICommand
    {
        public byte[] Data  { get; set; }

        public amIStorage()
        {
            base.Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, Service.Helper.GenerateCommandHandle<IStorageAccessor>(this) }
            };
        }

        public override void InitData(object obj)
        {
            Data = (byte[])obj;
        }
    }
}
