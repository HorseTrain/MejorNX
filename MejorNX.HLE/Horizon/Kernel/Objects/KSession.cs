using MejorNX.Common.Debugging;
using MejorNX.Common.Utilities;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Kernel.Objects
{
    public class KSession : KObject
    {
        public bool Opened                      { get; set; }
        public bool IsDomain                    { get; set; }

        public ICommand Commands                { get; set; }
        public ObjectCollection DomainObjects   { get; set; }

        public KSession(Process process,ICommand commands, string Name) : base(process, Name)
        {
            Commands = commands;
            DomainObjects = new ObjectCollection();

            Handle = process.ServiceHandles.AddObject(this);
        }

        public const long Sfci = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'I' << 24;
        public const long Sfco = 'S' << 0 | 'F' << 8 | 'C' << 16 | 'O' << 24;

        public void CallService(ServiceCallContext context)
        {
            ICommand commands = Commands;

            if (IsDomain)
            {
                int DomainWord0 = context.Reader.ReadStruct<int>();
                int DomainObjId = context.Reader.ReadStruct<int>();

                context.Reader.Advance(8);

                int DomainCmd = DomainWord0 & 0xff;

                switch (DomainCmd)
                {
                    case 1:

                        context.Writer.WriteStruct(0L);
                        context.Writer.WriteStruct(0L);

                        commands = (ICommand)DomainObjects.GetObject((uint)DomainObjId);

                        break;
                    case 2:

                        DomainObjects.DeleteObject((uint)DomainObjId);

                        context.Writer.WriteStruct(0L);

                        context.Ignore = true;
                        
                        break;
                    default: throw new NotImplementedException();
                }
            }

            if (context.Ignore)
            {
                return;
            }

            ulong Magic = context.Reader.ReadStruct<ulong>();
            ulong CommandID = context.Reader.ReadStruct<ulong>();

            ServiceCall call;

            if (commands.Calls.TryGetValue(CommandID, out call))
            {
                context.Writer.Seek(IsDomain ? 0x20UL : 0x10UL);

                context.Service = call;

                ulong Result = context.CallService();

                if (IsDomain)
                {
                    foreach (int ID in context.Response.Responses)
                    {
                        context.Writer.WriteStruct(ID);
                    }

                    context.Writer.Seek(0);

                    context.Writer.WriteStruct(context.Response.Responses.Count);
                }

                context.Writer.Seek(IsDomain ? 0x10UL : 0);

                context.Writer.WriteStruct(Sfco);
                context.Writer.WriteStruct(Result);
            }
            else
            {
                Debug.LogError($"Unknown Service {commands.GetType()} {Name} {CommandID}",true);
            }
        }

        public uint ConvertToDomain()
        {
            IsDomain = true;

            return DomainObjects.AddObject(Commands);
        }
    }
}
