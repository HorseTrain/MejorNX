using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.fspsrv
{
    public class IFileSystemProxy : ICommand
    {
        public IFileSystemProxy()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1,     SetCurrentProcess },
                {18,    Helper.GenerateCommandHandle<IFileSystem>(VirtualFS.FSContext.SdPath) },
                {51,    Helper.GenerateCommandHandle<IFileSystem>(VirtualFS.FSContext.SavePath) },
                {200,   Helper.GenerateCommandHandle<IStorage>(VirtualFS.FSContext.MainFSContext.cart.stream) },
                {1005,  GetGlobalAccessLogMode }
            };
        }

        ulong SetCurrentProcess(ServiceCallContext context)
        {
            return 0;
        }

        ulong GetGlobalAccessLogMode(ServiceCallContext context)
        {
            context.Writer.WriteStruct(0);

            return 0;
        }
    }
}
