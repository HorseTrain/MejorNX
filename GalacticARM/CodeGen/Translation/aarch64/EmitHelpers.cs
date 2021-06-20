using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public unsafe partial class Emit64
    {
        public static ulong SignExtendInt(int imm, int size) => (ulong)((((long)imm) << (64 - size)) >> (64 - size));

        public static Operand Shift(TranslationContext context, ShiftType Type, Operand Source, Operand Imm)
        {
            if (Imm.Type == OperandType.Immediate && Imm.Data == 0)
            {
                return context.CreateLocal(Source);
            }

            switch (Type)
            {
                case ShiftType.LSL: return context.ShiftLeft(Source, Imm);
                case ShiftType.LSR: return context.ShiftRight(Source, Imm);
                case ShiftType.ASR: return context.ShiftRight_Singed(Source, Imm);
                case ShiftType.ROR: return context.Or(context.ShiftRight(Source, Imm),context.ShiftLeft(Source, context.Subtract(context.Const(context.CurrentSize == IntSize.Int32 ? 32UL : 64UL), Imm)));
                default: return context.ThrowUnknown();
            }
        }

        public static Operand Extend(TranslationContext context, Operand Source, IntType Type)
        {
            switch (Type)
            {
                case IntType.Int8: return context.SignExtend8(Source);
                case IntType.Int16: return context.SignExtend16(Source);
                case IntType.Int32: return context.SignExtend32(Source);

                case IntType.UInt8: return context.And(Source,(ulong)byte.MaxValue);
                case IntType.UInt16: return context.And(Source,(ulong)ushort.MaxValue);
                case IntType.UInt32: return context.And(Source,uint.MaxValue);
            }

            return Source;
        }

        public static ulong GetFloatImm(double d)
        {
            return *(ulong*)(&d);
        }

        public static ulong GetFloatImm(float d)
        {
            return *(uint*)(&d);
        }

        public static ulong FloatImmOnSize(float f, int size)
        {
            if (size == 2)
                return GetFloatImm(f);
            else if (size == 3)
                return GetFloatImm((double)f);

            throw new NotImplementedException();
        }

        public static void CallFloatFallBack(TranslationContext context,string Name)
        {
            context.SetRegRaw(nameof(ExecutionContext.FunctionPointer),context.GetFunctionPointer(Name));

            context.CallRaw(context.GetRegRaw(nameof(ExecutionContext.FunctionPointer)));
        }
    }
}
