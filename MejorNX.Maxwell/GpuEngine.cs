using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public class GpuEngine
    {
        public MaxwellContext Gpu                           { get; set; }
        public int[] Registers                              { get; set; }
        public Dictionary<int,GpuCommandFunction> Calls     { get; set; }

        public GpuEngine(MaxwellContext gpu)
        {
            Gpu = gpu;

            Calls = new Dictionary<int, GpuCommandFunction>();
        }

        protected virtual void CallMethod(GpuCommand command)
        {
            if (Calls.TryGetValue(command.Method,out GpuCommandFunction Method))
            {
                Method(command);
            }
            else
            {
                WriteRegister(command);
            }
        }

        public void WriteRegister(GpuCommand command)
        {
            int ArgsCount = command.Arguments.Length;

            if (ArgsCount > 0)
            {
                Registers[command.Method] = command.Arguments[ArgsCount - 1];
            }
        }

        protected void AddCall(int ID, int Count, int Stride, GpuCommandFunction Call)
        {
            while (Count -- > 0)
            {
                Calls.Add(ID,Call);

                ID += Stride;
            }
        }

        //TODO: move "Reg" into enum.
        public ulong MakeInt64From2xInt32(int Reg)
        {
            return
                (ulong)Registers[(int)Reg + 0] << 32 |
                (uint)Registers[(int)Reg + 1];
        }
    }
}
