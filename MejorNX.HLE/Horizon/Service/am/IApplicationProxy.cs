using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IApplicationProxy : ICommand
    {
        public IApplicationProxy()
        {
            base.Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0,     Service.Helper.GenerateCommandHandle<ICommonStateGetter>()           },
                {1,     Service.Helper.GenerateCommandHandle<ISelfController>()              },
                {2,     Service.Helper.GenerateCommandHandle<IWindowController>()            },
                {3,     Service.Helper.GenerateCommandHandle<IAudioController>()             },
                {4,     Service.Helper.GenerateCommandHandle<IDisplayController>()           },
                //{10,    GlobalSession.GenerateCommandHandle<IProcessWindingController>()    },
                {11,    Service.Helper.GenerateCommandHandle<ILibraryAppletCreator>()        },
                {20,    Service.Helper.GenerateCommandHandle<IApplicationFunctions>()        },
                {1000,  Service.Helper.GenerateCommandHandle<IDebugFunctions>()              }
            };
        }
    }
}
