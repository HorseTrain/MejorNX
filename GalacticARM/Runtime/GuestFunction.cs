using GalacticARM.CodeGen.Translation;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime.X86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime
{
    public unsafe delegate ulong _func(ExecutionContext* context);

    public unsafe class GuestFunction
    {
        public byte[] Buffer        { get; set; }
        public _func Func           { get; set; }
        public ulong Ptr            { get; set; }

        public int TimesCalled              { get; set; }
        public Optimizations optimizations  { get; set; }

        public GuestFunction(byte[] Buffer)
        {
            this.Buffer = Buffer;

            lock (JitCache.Lock)
            {
                JitCache.GetNativeFunction(this);
            }

            Ptr = (ulong)Marshal.GetFunctionPointerForDelegate(Func);
        }

        public ulong Execute(ExecutionContext* context)
        {
            TimesCalled++;

            return Func(context);
        }

        public override string ToString()
        {
            SharpDisasm.Disassembler dis = new SharpDisasm.Disassembler(Buffer,SharpDisasm.ArchitectureMode.x86_64);

            StringBuilder Out = new StringBuilder();

            foreach (var ins in dis.Disassemble())
            {
                Out.AppendLine($"0x{ins.Offset:x3} {ins}");
            }

            return Out.ToString();
        }
    }
}
