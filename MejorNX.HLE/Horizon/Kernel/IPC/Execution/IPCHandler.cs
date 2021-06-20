using MejorNX.Cpu;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Service;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.IO;

namespace MejorNX.HLE.Horizon.Kernel.IPC.Execution
{
    public static class IPCHandler
    {
        public static void SendSyncRequest(CpuContext cpu, ulong Address,ulong Size, uint Handle)
        {
            KThread thread = (KThread)cpu.ThreadInformation;

            KSession session = (KSession)thread.Process.ServiceHandles[Handle];

            IPCCommand command = new IPCCommand(VirtualMemoryManager.GetReader(Address));

            ServiceCallContext context = new ServiceCallContext()
            {
                Session = session,
                Request = command,
                Response = new IPCCommand(),
                Reader = VirtualMemoryManager.GetReader(command.RawDataPointer),
                Writer = new BinaryBuilder(),
                process = thread.Process,
                CommandPointer = Address
            };

            IPCCall(context);

            cpu.X[0] = 0;
        }

        static void IPCCall(ServiceCallContext context)
        {
            switch (context.Request.Type)
            {
                case CommandType.Request: HandleIPCRequest(context); break;
                case CommandType.Control: HandleIPCControl(context); break;
                case CommandType.CloseSession: /* TODO: */ break;
                default: throw new NotImplementedException();
            }

            byte[] Response = context.Response.BuildResponse(context.CommandPointer);

            VirtualMemoryManager.GetWriter(context.CommandPointer).WriteStruct(Response);
        }

        public static void HandleIPCRequest(ServiceCallContext context)
        {
            context.Response.Type = CommandType.Response;

            context.Session.CallService(context);

            context.Response.RawData = context.Writer.GetBuffer();
        }

        public static void HandleIPCControl(ServiceCallContext context)
        {
            ulong Magic = context.Reader.ReadStruct<ulong>();
            ulong CommandID = context.Reader.ReadStruct<ulong>();

            switch (CommandID)
            {
                case 0: context.Request = FillResponse(context.Response,0,(int)context.Session.ConvertToDomain()); break;
                case 2: break;
                case 3: context.Request = FillResponse(context.Response, 0, 0x500); break; //Wtf?
                case 4:

                    //I'm assuming this just openes a new handle ?

                    context.Reader.Advance(4);

                    uint Handle = context.process.ServiceHandles.AddObject(context.Session);

                    context.Response.HandleDescriptor = HandleDescriptor.MakeMove(Handle);

                    context.Request = FillResponse(context.Response,0);

                    break;

                default: throw new NotImplementedException(CommandID.ToString());
            }
        }

        private const long SfcoMagic = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'O' << 24;

        public static IPCCommand FillResponse(IPCCommand Response, ulong Result, byte[] Data = null)
        {
            Response.Type = CommandType.Response;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(SfcoMagic);
                Writer.Write(Result);

                if (Data != null)
                {
                    Writer.Write(Data);
                }

                Response.RawData = MS.ToArray();
            }

            return Response;
        }

        public static IPCCommand FillResponse(IPCCommand Response, ulong Result, params int[] Values)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                foreach (int Value in Values)
                {
                    Writer.Write(Value);
                }

                return FillResponse(Response, Result, MS.ToArray());
            }
        }
    }
}
