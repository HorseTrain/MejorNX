using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.IPC
{
    public enum CommandType : short
    {
        Response = 0,
        CloseSession = 2,
        Request = 4,
        Control = 5
    }
}
