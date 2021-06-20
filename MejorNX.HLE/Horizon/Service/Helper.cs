using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Kernel.SVC;
using MejorNX.HLE.Horizon.Service.acc;
using MejorNX.HLE.Horizon.Service.am;
using MejorNX.HLE.Horizon.Service.fspsrv;
using MejorNX.HLE.Horizon.Service.lm;
using MejorNX.HLE.Horizon.Service.pctl;
using MejorNX.HLE.VirtualFS;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace MejorNX.HLE.Horizon.Service
{
    public static class Helper
    {
        public static ulong Make(ServiceCallContext context)
        {
            if (context.Session.IsDomain)
            {
                context.Response.Responses.Add((int)context.Session.DomainObjects.AddObject(context.Data));
            }
            else
            {
                KSession session = new KSession(context.process, (ICommand)context.Data, context.Session.Name);

                context.Response.HandleDescriptor = HandleDescriptor.MakeMove(session.Handle);
            }

            return 0;
        }

        public static ServiceCall GenerateCommandHandle<T>(object data = null) where T: ICommand, new()
        {
            return delegate (ServiceCallContext context) 
            { 
                T obj = new T();

                context.Data = obj;
                
                Make(context); 

                if (data != null)
                {
                    obj.InitData(data);
                }
                
                return 0;         
            };
        }
    }
}
