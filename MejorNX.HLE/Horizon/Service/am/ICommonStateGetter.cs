using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class ICommonStateGetter : ICommand
    {
        public ICommonStateGetter()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0, GetEventHandle },
                {1, ReceiveMessage },
                {9, GetCurrentFocusState }
            };
        }

        ulong GetEventHandle(ServiceCallContext context)
        {
            KSyncObject Event = context.process.AppletManager.Event;

            context.Response.HandleDescriptor = HandleDescriptor.MakeCopy(Event.Handle);

            return 0;
        }

        ulong ReceiveMessage(ServiceCallContext context)
        {
            //TODO: Check if you aren't able to pop

            AppletMessage message = context.process.AppletManager.PopMessage();

            context.Writer.WriteStruct((int)message);

            return 0;
        }

        ulong GetCurrentFocusState(ServiceCallContext context)
        {
            context.Writer.WriteStruct(context.process.AppletManager.GetFocusedState());

            return 0;
        }
    }
}
