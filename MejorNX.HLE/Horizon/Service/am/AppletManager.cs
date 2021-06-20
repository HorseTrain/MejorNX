using MejorNX.HLE.Horizon.Kernel.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.am
{
    public enum AppletMessage 
    {
        FocusStateChanged = 15
    }

    public class AppletManager 
    {
        public KSyncObject Event                { get; set; }
        public List<AppletMessage> Messages     { get; set; }
        public bool InFocus                     { get; set; }

        public AppletManager(Process process)
        {
            Event = new KSyncObject(process);
            Messages = new List<AppletMessage>();
        }

        public void SetFocus(bool focused)
        {
            InFocus = focused;

            PushMessage(AppletMessage.FocusStateChanged);
        }

        public void PushMessage(AppletMessage message)
        {
            Messages.Add(message);

            Event.Send();
        }

        public AppletMessage PopMessage()
        {
            AppletMessage Out = Messages[Messages.Count - 1];

            Messages.RemoveAt(Messages.Count - 1);

            return Out;
        }

        public byte GetFocusedState()
        {
            if (InFocus)
                return 1;
            else
                return 2;
        }
    }
}
