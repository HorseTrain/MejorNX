using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public class CommandStack
    {
        List<GpuCommand> Commands     { get; set; }

        NvGpuEngine[] Channels          { get; set; }
        int CurrentMacroPosition        { get; set; }
        int CurrMacroBindIndex          { get; set; }
        int[] Mme                       { get; set; }
        public CachedMacro[] Macros     { get; set; }

        public CommandStack()
        {
            Commands = new List<GpuCommand>();

            Channels = new NvGpuEngine[8]; //Why 8?

            Mme = new int[65536];
            Macros = new CachedMacro[128];
        }

        public void PushCommandCollection(GpuCommand[] Commands)
        {
            foreach (GpuCommand command in Commands)
            {
                PushCommand(command);
            }
        }

        void PushCommand(GpuCommand command)
        {
            lock (Commands)
            {
                Commands.Add(command);
            }
        }

        public void ExecuteCommands(MaxwellContext context)
        {
            lock (Commands)
            {
                for (int i = 0; i < Commands.Count; i++)
                {
                    GpuCommand command = Commands[i];

                    ExecuteCommand(command,context);
                }

                Commands = new List<GpuCommand>();
            }
        }

        void ExecuteCommand(GpuCommand command, MaxwellContext context)
        {
            if (command.Method < 0x80)
            {
                switch((NvGpuFifoMeth)command.Method)
                {
                    case NvGpuFifoMeth.BindChannel:
                    {
                        NvGpuEngine engine = (NvGpuEngine)command.Arguments[0];

                        Channels[command.SubChannel] = engine;

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroUploadAddress:
                    {
                        CurrentMacroPosition = command.Arguments[0];

                        break;
                    }

                    case NvGpuFifoMeth.SendMacroCodeData:
                    {
                        foreach (int arg in command.Arguments)
                        {
                            Mme[CurrentMacroPosition++] = arg;
                        }

                        break;
                    }

                    case NvGpuFifoMeth.SetMacroBindingIndex:
                    {
                        CurrentMacroPosition = command.Arguments[0];

                        break;
                    }

                    case NvGpuFifoMeth.BindMacro:
                    {
                        int Position = command.Arguments[0];

                        Macros[CurrMacroBindIndex] = new CachedMacro();

                        break;
                    }

                    //default: throw new NotImplementedException();
                }
            }
            else
            {
                switch (Channels[command.SubChannel])
                {
                    case NvGpuEngine._2d: context._2dEngine.Call(command);      break;
                    case NvGpuEngine._3d: context._3dEngine.Call(this,command); break;
                    case NvGpuEngine.Dma: context.dmaEngine.Call(command);      break;
                }
            }
        }
    }
}
