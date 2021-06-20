using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public static partial class Emit64
    {
        public static void CalculateNZ(TranslationContext context,Operand d)
        {
            context.SetRegRaw(nameof(ExecutionContext.Z),context.Ceq(d,0));
            context.SetRegRaw(nameof(ExecutionContext.N),context.Clz(d));
        }

        public static void SetAddsFlags(TranslationContext context, Operand d, Operand n, Operand m)
        {
            context.SetRegRaw(nameof(ExecutionContext.C), context.Clt_Un(d,n));
            context.SetRegRaw(nameof(ExecutionContext.V), context.Clz(context.And(context.Xor(d,n),context.Not(context.Xor(n,m)))));
        }

        public static void SetSubsFlags(TranslationContext context, Operand d, Operand n, Operand m)
        {
            context.SetRegRaw(nameof(ExecutionContext.C), context.Cgte_Un(n,m));
            context.SetRegRaw(nameof(ExecutionContext.V), context.Clz(context.And(context.Xor(d, n), context.Xor(n, m))));
        }

        public static void SetFlagsImm(TranslationContext context, ulong Imm)
        {
            context.SetRegRaw(nameof(ExecutionContext.N), (Imm >> 3) & 1);
            context.SetRegRaw(nameof(ExecutionContext.Z), (Imm >> 2) & 1);
            context.SetRegRaw(nameof(ExecutionContext.C), (Imm >> 1) & 1);
            context.SetRegRaw(nameof(ExecutionContext.V), (Imm >> 0) & 1);
        }
    }
}
