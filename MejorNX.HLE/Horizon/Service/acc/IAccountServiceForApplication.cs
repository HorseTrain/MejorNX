using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.acc
{
    public class IAccountServiceForApplication : ICommand
    {
        public IAccountServiceForApplication()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {0,     GetUserCount },
                {1,     GetUserExistence },
                {5,     Helper.GenerateCommandHandle<IProfile>() },
                {100,   InitializeApplicationInfo },
                {101,   Helper.GenerateCommandHandle<IManagerForApplication>() }
            };
        }

        ulong GetUserCount(ServiceCallContext context)
        {
            context.Writer.WriteStruct(0);

            context.PrintStubbed();

            return 0;
        }

        ulong GetUserExistence(ServiceCallContext context)
        {
            context.Writer.WriteStruct(1);

            context.PrintStubbed();

            return 0;
        }

        ulong InitializeApplicationInfo(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
