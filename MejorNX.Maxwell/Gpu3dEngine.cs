using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public unsafe class Gpu3dEngine : GpuEngine
    {
        public Gpu3dEngine(MaxwellContext context) : base(context)
        {
            Registers = new int[3584];

            AddCall(0x585, 1, 1, VertexEndGl);
            AddCall(0x6c3, 1,1, QueryControl);
        }

        public void Call(CommandStack fifocontext, GpuCommand command)
        {
            if (command.Method < 0xe00)
            {
                CallMethod(command);
            }
            else
            {
                //TODO:

                int MacroIndex = (command.Method >> 1) & 0x7F;

                if ((command.Method & 1) != 0)
                {
                    foreach (int Arg in command.Arguments)
                    {

                    }
                }
                else
                {

                }
            }
        }

        void VertexEndGl(GpuCommand command)
        {


        }

        void QueryControl(GpuCommand command)
        {
            ulong Position = MakeInt64From2xInt32(0x6c0);

            int Seq = Registers[0x6c2];
            int Ctrl = Registers[0x6c3];

            int Mode = Ctrl & 3;

            if (Mode == 0)
            {
                ulong PA = Gpu.Vmm.GetPhysicalAddress(Position);

                *(int*)(Gpu.Vmm.BaseAddress + PA) = Seq;
            }

            WriteRegister(command);
        }
    }
}
