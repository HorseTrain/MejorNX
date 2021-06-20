using System.Collections.Generic;

namespace MejorNX.HLE.Horizon.Service.am
{
    public class IApplicationFunctions : ICommand
    {
        public IApplicationFunctions()
        {
            base.Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1,     Service.Helper.GenerateCommandHandle<amIStorage>(Helper.MakeLaunchParams())},
                {20,    EnsureSaveData},
                {21,    GetDesiredLanguage},
                {23,    GetDisplayVersion},
                {40,    NotifyRunning },
                {66,    InitializeGamePlayRecording},
                {67,    SetGamePlayRecordingState}
            };
        }

        ulong EnsureSaveData(ServiceCallContext context)
        {
            context.PrintStubbed();

            context.Reader.ReadStruct<ulong>();
            context.Reader.ReadStruct<ulong>();

            context.Writer.WriteStruct(0L);

            return 0;
        }

        ulong GetDesiredLanguage(ServiceCallContext context)
        {
            context.Writer.WriteStruct<ulong>(357911326309);

            return 0;
        }

        ulong GetDisplayVersion(ServiceCallContext context)
        {
            context.Writer.WriteStruct(1L);
            context.Writer.WriteStruct(0L);

            return 0;
        }

        ulong NotifyRunning(ServiceCallContext context)
        {
            //context.PrintStubbed();

            context.Writer.WriteStruct(1);

            return 0;
        }

        ulong InitializeGamePlayRecording(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }

        ulong SetGamePlayRecordingState(ServiceCallContext context)
        {
            context.PrintStubbed();

            return 0;
        }
    }
}
