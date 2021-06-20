using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public static partial class Emit64
    {
        public static void AddsSubs_ExtendedReg(TranslationContext context) => AddSubtractExtend(context, context.GetRaw("op") == 0, true);
        public static void AddsSubs_Imm(TranslationContext context) => AddSubtractImm(context, context.GetRaw("op") == 0, true);
        public static void AddSub_ExtendedReg(TranslationContext context) => AddSubtractExtend(context, context.GetRaw("op") == 0, false);
        public static void AddSub_Imm(TranslationContext context) => AddSubtractImm(context, context.GetRaw("op") == 0, false);
        public static void AddSub_ShiftedReg_s(TranslationContext context) => AddSubtractShiftedRegister(context, context.GetRaw("op") == 0, context.GetRaw("s") == 1);
        public static void And_Imm(TranslationContext context) => LogicalImm(context, 0);
        public static void And_ShiftedReg(TranslationContext context) => LogicalShiftedRegister(context, 0);
        public static void Eor_Imm(TranslationContext context) => LogicalImm(context, 2);
        public static void Eor_ShiftedReg(TranslationContext context) => LogicalShiftedRegister(context, 2);
        public static void Orr_Imm(TranslationContext context) => LogicalImm(context, 1);
        public static void Orr_ShiftedReg(TranslationContext context) => LogicalShiftedRegister(context, 1);
        public static void Movn(TranslationContext context) => Mov(context, true);
        public static void Movz(TranslationContext context) => Mov(context, false);
        public static void Madd(TranslationContext context) => Mul(context, true);
        public static void Msub(TranslationContext context) => Mul(context, false);
        public static void Sdiv(TranslationContext context) => Div(context, true);
        public static void Smaddl(TranslationContext context) => MultiplyLong(context, true, true);
        public static void Smsubl(TranslationContext context) => MultiplyLong(context, false, true);
        public static void Smulh(TranslationContext context) => MultiplyHi(context, true);
        public static void Udiv(TranslationContext context) => Div(context, false);
        public static void Umaddl(TranslationContext context) => MultiplyLong(context, true, false);
        public static void Umsubl(TranslationContext context) => MultiplyLong(context, false, false);
        public static void Umulh(TranslationContext context) => MultiplyHi(context, false);
        public static void ASRV(TranslationContext context) => ShiftV(context, ShiftType.ASR);
        public static void LSLV(TranslationContext context) => ShiftV(context, ShiftType.LSL);
        public static void LSRV(TranslationContext context) => ShiftV(context, ShiftType.LSR);
        public static void RORV(TranslationContext context) => ShiftV(context, ShiftType.ROR);

        public static void AddSubtractImm(TranslationContext context, bool IsAdd, bool SetFlags)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRaw("imm") << (context.GetRaw("shift") * 12);

            AddSubtract(context, n, m, IsAdd, SetFlags);
        }

        public static void AddSubtractShiftedRegister(TranslationContext context, bool IsAdd, bool SetFlags)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            int imm = context.GetRaw("imm");
            ShiftType Type = (ShiftType)context.GetRaw("shift");

            m = Shift(context, Type, m, imm);

            AddSubtract(context, n, m, IsAdd, SetFlags);
        }

        public static void AddSubtractExtend(TranslationContext context, bool IsAdd, bool SetFlags)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            int shift = context.GetRaw("shift");
            IntType Type = (IntType)context.GetRaw("option");

            m = context.ShiftLeft(Extend(context, m, Type), shift);

            AddSubtract(context, n, m, IsAdd, SetFlags);
        }

        static void AddSubtract(TranslationContext context, Operand n, Operand m, bool IsAdd, bool SetFlags)
        {
            Operand d;

            if (IsAdd)
            {
                d = context.Add(n,m);
            }   
            else
            {
                d = context.Subtract(n,m);
            }

            if (SetFlags)
            {
                CalculateNZ(context,d);

                if (IsAdd)
                {
                    SetAddsFlags(context, d, n, m);
                }
                else
                {
                    SetSubsFlags(context, d, n, m);
                }
            }

            context.SetRegister("rd",d);
        }

        public static void Extr(TranslationContext context)
        {
            context.SetSize("sf");

            int imms = context.GetRaw("imms");

            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            Operand res = context.GetRegister("rm");

            if (imms != 0)
            {
                if (rn == rm)
                {
                    res = Shift(context, ShiftType.ROR, res, imms);
                }
                else
                {
                    res = context.ShiftRight(res,imms);

                    Operand n = context.GetRegister("rn");

                    int invShift = (context.CurrentSize == IntSize.Int32 ? 32 : 64) - imms;

                    res = context.Or(res, context.ShiftLeft(n,invShift));
                }
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister("rd", res);
        }

        public static void Bfm(TranslationContext context)
        {
            context.SetSize("sf");

            var mask = DecoderHelper.DecodeBitMask(context.CurrentOpCode.RawOpCode, false);

            Operand dst = context.GetRegister("rd");
            Operand src = context.GetRegister("rn");

            int r = context.GetRaw("immr");
            int s = context.GetRaw("imms");

            Operand bot = context.Or( context.And(dst,~mask.WMask) , context.And( Shift(context,ShiftType.ROR,src,r) , mask.WMask));

            context.SetRegister("rd",context.Or(context.And(dst,~mask.TMask),context.And(bot,mask.TMask)));
        }

        public static void Sbfm(TranslationContext context)
        {
            context.SetSize("sf");

            Operand src = context.GetRegister("rn");
            int r = context.GetRaw("immr");
            int s = context.GetRaw("imms");

            var mask = DecoderHelper.DecodeBitMask(context.CurrentOpCode.RawOpCode, false);

            Operand bot = context.And(Shift(context, ShiftType.ROR, src, r), mask.WMask);

            Operand top = context.Subtract(context.Const(0) , context.And(context.ShiftRight(src,s),1));

            context.SetRegister("rd", context.Or(context.And(top,~mask.TMask),context.And(bot,mask.TMask)));
        }

        public static void Ubfm(TranslationContext context)
        {
            context.SetSize("sf");

            Operand src = context.GetRegister("rn");
            int r = context.GetRaw("immr");
            int s = context.GetRaw("imms");

            var mask = DecoderHelper.DecodeBitMask(context.CurrentOpCode.RawOpCode, false);

            Operand bot = context.And(Shift(context, ShiftType.ROR, src, r), mask.WMask);

            context.SetRegister("rd", context.And(bot , mask.TMask));
        }

        static void LogicalImm(TranslationContext context, int opc)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            ulong Imm = (ulong)DecoderHelper.DecodeBitMask(context.CurrentOpCode.RawOpCode, true).WMask;

            Operand d;

            switch (opc)
            {
                case 0: d = context.And(n , Imm); break;
                case 1: d = context.Or(n, Imm); break;
                case 2: d = context.Xor(n,Imm); break;
                default: d = context.ThrowUnknown(); break;
            }

            context.SetRegister("rd", d);
        }

        public static void LogicalShiftedRegister(TranslationContext context, int opc)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            ShiftType Type = (ShiftType)context.GetRaw("shift");
            int Shift = context.GetRaw("imm");

            bool Not = context.GetRaw("n") == 1;

            m = Emit64.Shift(context, Type, m, Shift);

            if (Not)
                m = context.Not(m);

            Operand d;

            switch (opc)
            {
                case 0: d = context.And(n, m); break;
                case 1: d = context.Or(n, m); break;
                case 2: d = context.Xor(n, m); break;
                default: d = context.ThrowUnknown(); break;
            }

            context.SetRegister("rd", d);
        }

        public static void Ands_ShiftedReg(TranslationContext context)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            ShiftType Type = (ShiftType)context.GetRaw("shift");
            int Shift = context.GetRaw("imm");
            bool IsBic = context.GetRaw("n") == 1;

            m = Emit64.Shift(context, Type, m, Shift);

            if (IsBic)
                m = context.Not(m);

            Operand d = context.And(n , m);

            SetFlagsImm(context,0);
            CalculateNZ(context, d);

            context.SetRegister("rd", d);
        }

        public static void Ands_Imm(TranslationContext context)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            ulong Imm = (ulong)DecoderHelper.DecodeBitMask(context.CurrentOpCode.RawOpCode, true).WMask;

            Operand d = context.And(n , Imm);

            SetFlagsImm(context, 0);
            CalculateNZ(context, d);

            context.SetRegister("rd", d);
        }

        public static void Mov(TranslationContext context, bool Not)
        {
            context.SetSize("sf");

            int hw = context.GetRaw("hw") * 16;
            ulong imm = (ulong)context.GetRaw("imm");

            imm <<= hw;

            if (Not)
                imm = ~imm;

            context.SetRegister("rd", imm);
        }

        public static void Movk(TranslationContext context)
        {
            context.SetSize("sf");

            int hw = context.GetRaw("hw") * 16;
            ulong imm = (ulong)context.GetRaw("imm");

            imm <<= hw;

            Operand d = context.GetRegister("rd");

            d = context.And(d,~(((ulong)ushort.MaxValue) << hw));
            d = context.Or(d, imm);

            context.SetRegister("rd", d);
        }

        static void Mul(TranslationContext context, bool IsAdd)
        {
            context.SetSize("sf");

            Operand a = context.GetRegister("ra");
            Operand m = context.GetRegister("rm");
            Operand n = context.GetRegister("rn");

            Operand mul = context.Multiply(n,m);

            if (IsAdd)
                mul = context.Add(a , mul);
            else
                mul = context.Subtract(a, mul);

            context.SetRegister("rd", mul);
        }

        public static void MultiplyHi(TranslationContext context, bool Signed)
        {
            Operand m = context.GetRegister("rm");
            Operand n = context.GetRegister("rn");

            Operand d = context.Call(nameof(Fallbackbits.MulH), n, m, Signed ? 1 : 0);

            context.SetRegister("rd", d);
        }

        static void Div(TranslationContext context, bool Signed)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            EmitUniversal.EmitIf(context,

                context.Ceq(m, 0),

                delegate ()
                {
                    context.SetRegister("rd", 0);
                },

                delegate ()
                {
                    Instruction instruction = Signed ? Instruction.Divide : Instruction.Divide_Un;

                    context.SetRegister("rd", context.MoveWithOperation(instruction,n,m));
                }

                );
        }

        static void MultiplyLong(TranslationContext context, bool IsAdd, bool Signed)
        {
            Operand a = context.GetRegister("ra");
            Operand m = context.GetRegister("rm");
            Operand n = context.GetRegister("rn");

            if (Signed)
            {
                m = Extend(context,m, IntType.Int32);
                n = Extend(context,n, IntType.Int32);
            }
            else
            {
                m = Extend(context,m, IntType.UInt32);
                n = Extend(context,n, IntType.UInt32);
            }

            Operand d;

            if (IsAdd)
            {
                d = context.Add(a , context.Multiply(n, m));
            }
            else
            {
                d = context.Subtract(a, context.Multiply(n, m));
            }

            context.SetRegister("rd", d);
        }

        public static void Adr(TranslationContext context)
        {
            ulong Imm = (ulong)((((long)context.CurrentOpCode.RawOpCode << 40) >> 43) & ~3) | (ulong)(((long)context.CurrentOpCode.RawOpCode >> 29) & 3);

            Imm += context.CurrentOpCode.Address;

            context.SetRegister("rd", Imm);
        }

        public static void Adrp(TranslationContext context)
        {
            ulong Imm = (ulong)((((long)context.CurrentOpCode.RawOpCode << 40) >> 43) & ~3) | (ulong)(((long)context.CurrentOpCode.RawOpCode >> 29) & 3);

            Imm <<= 12;

            Imm += context.CurrentOpCode.Address;

            Imm &= ~4095UL;

            context.SetRegister("rd", Imm);
        }

        public static void Rbit(TranslationContext context)
        {
            context.SetSize("sf");

            Operand value = context.GetRegister("rn");

            if (context.CurrentSize == IntSize.Int32)
            {
                value = context.Or(context.ShiftRight(context.And(value, 0xaaaaaaaa), 1), context.ShiftLeft(context.And(value, 0x55555555), 1));
                value = context.Or(context.ShiftRight(context.And(value, 0xcccccccc), 2), context.ShiftLeft(context.And(value, 0x33333333), 2));
                value = context.Or(context.ShiftRight(context.And(value, 0xf0f0f0f0), 4), context.ShiftLeft(context.And(value, 0x0f0f0f0f), 4));
                value = context.Or(context.ShiftRight(context.And(value, 0xff00ff00), 8), context.ShiftLeft(context.And(value, 0x00ff00ff), 8));

                value = context.Or(context.ShiftRight(value, 16), context.ShiftLeft(value, 16));
            }
            else if (context.CurrentSize == IntSize.Int64)
            {
                value = context.Or(context.ShiftRight(context.And(value, 0xaaaaaaaaaaaaaaaa), 1), context.ShiftLeft(context.And(value, 0x5555555555555555), 1));
                value = context.Or(context.ShiftRight(context.And(value, 0xcccccccccccccccc), 2), context.ShiftLeft(context.And(value, 0x3333333333333333), 2));
                value = context.Or(context.ShiftRight(context.And(value, 0xf0f0f0f0f0f0f0f0), 4), context.ShiftLeft(context.And(value, 0x0f0f0f0f0f0f0f0f), 4));
                value = context.Or(context.ShiftRight(context.And(value, 0xff00ff00ff00ff00), 8), context.ShiftLeft(context.And(value, 0x00ff00ff00ff00ff), 8));
                value = context.Or(context.ShiftRight(context.And(value, 0xffff0000ffff0000), 16), context.ShiftLeft(context.And(value,0x0000ffff0000ffff), 16));

                value = context.Or(context.ShiftRight(value, 32), context.ShiftLeft(value, 32));
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister("rd", value);
        }

        public static void Clz(TranslationContext context)
        {
            context.SetSize("sf");

            Operand value = context.GetRegister("rn");

            value = context.Call(nameof(Fallbackbits.CountLeadingZeros), value, context.CurrentSize == IntSize.Int32 ? 32 : 64);

            context.SetRegister("rd", value);
        }

        public static void ShiftV(TranslationContext context, ShiftType Type)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            m = context.And(m,context.CurrentSize == IntSize.Int32 ? 31 : 63);

            Operand d = Shift(context, Type, n, m);

            context.SetRegister("rd", d);
        }

        public static void Rev(TranslationContext context)
        {
            context.SetSize("sf");

            int sf = context.GetRaw("sf");

            int opc = context.GetRaw("opc");

            Operand value = context.GetRegister("rn");

            Operand Out = context.Const(0);

            if (sf == 0 && opc == 0)
            {
                Out = context.Call(nameof(Fallbackbits.Rev),value,0);
            }
            else if (sf == 1 && opc == 1)
            {
                Out = context.Call(nameof(Fallbackbits.Rev), value, 1);
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister("rd",Out);
        }

        public static void Rev16(TranslationContext context)
        {
            context.SetSize("sf");

            int sf = context.GetRaw("sf");

            Operand n = context.GetRegister("rn");

            Operand RevShort(Operand value)
            {
                return context.Or(context.ShiftLeft(context.And(value,0xFFU) ,8) , context.ShiftRight(context.And(value,0xFF00U) ,8));
            }

            Operand Out = RevShort(n);

            if (sf == 0)
            {
                Out = context.Or(Out,context.ShiftLeft(RevShort(context.ShiftRight(n,16)),16));
            }
            else if (sf == 1)
            {
                Out = context.Or(Out, context.ShiftLeft(RevShort(context.ShiftRight(n, 16)), 16));
                Out = context.Or(Out, context.ShiftLeft(RevShort(context.ShiftRight(n, 32)), 32));
                Out = context.Or(Out, context.ShiftLeft(RevShort(context.ShiftRight(n, 48)), 48));
            }
            else
            {
                context.ThrowUnknown();
            }

            context.SetRegister("rd",Out);
        }
    }
}
