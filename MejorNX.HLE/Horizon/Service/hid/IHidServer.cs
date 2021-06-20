using MejorNX.HLE.Horizon.Service.nv;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.hid
{
    public class IHidServer : ICommand
    {
        public IHidServer()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                { 0,   Helper.GenerateCommandHandle<IAppletResource>(Switch.MainSwitch.Hos.HidSharedMemory)                         },                
                { 1,   ActivateDebugPad                                                                                             },
                { 11,  ActivateTouchScreen                                                                                          },
                { 21,  ActivateMouse                                                                                                },
                { 31,  ActivateKeyboard                                                                                             },
                { 66,  StartSixAxisSensor                                                                                           },
                { 79,  SetGyroscopeZeroDriftMode                                                                                    },
                { 100, SetSupportedNpadStyleSet                                                                                     },
                { 101, GetSupportedNpadStyleSet                                                                                     },
                { 102, SetSupportedNpadIdType                                                                                       },
                { 103, ActivateNpad                                                                                                 },
                { 108, GetPlayerLedPattern                                                                                          },
                { 120, SetNpadJoyHoldType                                                                                           },
                { 121, GetNpadJoyHoldType                                                                                           },
                { 122, SetNpadJoyAssignmentModeSingleByDefault                                                                      },
                { 123, SetNpadJoyAssignmentModeSingle                                                                               },
                { 124, SetNpadJoyAssignmentModeDual                                                                                 },
                { 125, MergeSingleJoyAsDualJoy                                                                                      },
                { 128, SetNpadHandheldActivationMode                                                                                },
                { 200, GetVibrationDeviceInfo                                                                                       },
                { 201, SendVibrationValue                                                                                           },
                { 203, Helper.GenerateCommandHandle<IActiveApplicationDeviceList>()                                                 },
                { 206, SendVibrationValues                                                                                          }
            };
        }

        public ulong CreateAppletResource(ServiceCallContext Context)
        {
            return 0;
        }

        public ulong ActivateDebugPad(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong ActivateTouchScreen(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong ActivateMouse(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong ActivateKeyboard(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong StartSixAxisSensor(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetGyroscopeZeroDriftMode(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong GetSupportedNpadStyleSet(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetSupportedNpadStyleSet(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetSupportedNpadIdType(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong ActivateNpad(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong GetPlayerLedPattern(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetNpadJoyHoldType(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong GetNpadJoyHoldType(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetNpadJoyAssignmentModeSingleByDefault(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetNpadJoyAssignmentModeSingle(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetNpadJoyAssignmentModeDual(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong MergeSingleJoyAsDualJoy(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SetNpadHandheldActivationMode(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong GetVibrationDeviceInfo(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            Context.Writer.WriteStruct(0L);

            return 0;
        }

        public ulong SendVibrationValue(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong CreateActiveVibrationDeviceList(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }

        public ulong SendVibrationValues(ServiceCallContext Context)
        {
            Context.PrintStubbed();

            return 0;
        }
    }
}
