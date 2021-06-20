using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    class OpCodeSystem
    {
        public int Rt { get; private set; }
        public int Op2 { get; private set; }
        public int CRm { get; private set; }
        public int CRn { get; private set; }
        public int Op1 { get; private set; }
        public int Op0 { get; private set; }

        public void Load(int opCode)
        {
            Rt = (opCode >> 0) & 0x1f;
            Op2 = (opCode >> 5) & 0x7;
            CRm = (opCode >> 8) & 0xf;
            CRn = (opCode >> 12) & 0xf;
            Op1 = (opCode >> 16) & 0x7;
            Op0 = ((opCode >> 19) & 0x1) | 2;
        }
    }

    public static partial class Emit64
    {
        private static int GetPackedId(OpCodeSystem op)
        {
            int id;

            id = op.Op2 << 0;
            id |= op.CRm << 3;
            id |= op.CRn << 7;
            id |= op.Op1 << 11;
            id |= op.Op0 << 14;

            return id;
        }

        public static void Svc(TranslationContext context)
        {
            int imm = context.GetRaw("imm");

            context.Call(nameof(CpuThread.CallSVC), context.ContextPointer(), imm);

            context.AdvancePC();
        }

        public static void Mrs(TranslationContext context)
        {
            OpCodeSystem opCode = new OpCodeSystem();

            opCode.Load(context.CurrentOpCode.RawOpCode);

            Operand d;

            switch (GetPackedId(opCode))
            {
                case 0b11_011_0000_0000_001: d = 0x8444c004; break;
                case 0b11_011_0000_0000_111: d = 0x00000004; break;
                case 0b11_011_0100_0100_000: d = context.GetRegRaw(nameof(ExecutionContext.fpcr)); break;
                case 0b11_011_0100_0100_001: d = context.GetRegRaw(nameof(ExecutionContext.fpsr)); break;
                case 0b11_011_1101_0000_010: d = context.GetRegRaw(nameof(ExecutionContext.tpidr)); break;
                case 0b11_011_1101_0000_011: d = context.GetRegRaw(nameof(ExecutionContext.tpidrro_el0)); break;
                //case 0b11_011_1110_0000_000: d = context.GetFieldRaw(nameof(ThreadContext.c)); break;
                case 0b11_011_1110_0000_001: d = context.Call(nameof(FallbackOther.GetCntpctEl0)); break;
                default: d = context.ThrowUnknown(); break;
            }

            context.SetRegister("rt", d);

            context.AdvancePC();
        }

        public static void Msr(TranslationContext context)
        {
            OpCodeSystem opCode = new OpCodeSystem();

            opCode.Load(context.CurrentOpCode.RawOpCode);

            Operand src = context.GetRegister("rt");

            switch (GetPackedId(opCode))
            {
                case 0b11_011_0100_0100_000: context.SetRegRaw(nameof(ExecutionContext.fpcr), src); break;
                case 0b11_011_0100_0100_001: context.SetRegRaw(nameof(ExecutionContext.fpsr), src); break;
                case 0b11_011_1101_0000_010: context.SetRegRaw(nameof(ExecutionContext.tpidr), src); break;

                default: context.ThrowUnknown(); break;
            }

            context.AdvancePC();
        }

        public static void Nop(TranslationContext context)
        {
            context.AdvancePC();
        }
    }
}
