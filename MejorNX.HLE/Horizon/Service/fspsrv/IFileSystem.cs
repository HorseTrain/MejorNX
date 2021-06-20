using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.fspsrv
{
    public class IFileSystem : ICommand
    {
        string Path = "";

        public IFileSystem()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {

            };
        }

        public override void InitData(object obj)
        {
            Path = (string)obj;
        }
    }
}
