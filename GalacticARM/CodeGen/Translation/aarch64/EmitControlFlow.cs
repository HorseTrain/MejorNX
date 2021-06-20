using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using static GalacticARM.CodeGen.Translation.EmitUniversal;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public partial class Emit64
    {
        public static void B(TranslationContext context) => B_Imm(context,false);
        public static void BL(TranslationContext context) => B_Imm(context, true);

        public static void BrRet(TranslationContext context) => context.SetReturn(context.GetRegister("rn"));

        public static void Blr(TranslationContext context)
        {
            context.SetRegister(30, context.CurrentOpCode.Address + 4);

            BrRet(context);
        }

        static void B_Imm(TranslationContext context, bool SetLR)
        {
            int imm = context.GetRaw("imm");

            ulong NewAddress = context.CurrentOpCode.Address + (SignExtendInt(imm, 26) << 2);

            if (SetLR)
            {
                context.SetRegister(30,context.CurrentOpCode.Address + 4);
            }

            context.SetReturn(NewAddress);
        }

        public static void B_Cond(TranslationContext context)
        {
            int imm = context.GetRaw("imm");

            ulong NewAddress = context.CurrentOpCode.Address + (SignExtendInt(imm, 19) << 2);

            Condition condition = (Condition)context.GetRaw("cond");

            EmitIf(context, ConditionHolds(context, condition),

                delegate ()
                {
                    context.SetReturn(NewAddress);
                },

                delegate ()
                {
                    context.AdvancePC();
                }


                );
        }

        public static Operand ConditionHolds(TranslationContext context, Condition cond)
        {
            Operand N = context.GetRegRaw(nameof(ExecutionContext.N));
            Operand Z = context.GetRegRaw(nameof(ExecutionContext.Z));
            Operand C = context.GetRegRaw(nameof(ExecutionContext.C));
            Operand V = context.GetRegRaw(nameof(ExecutionContext.V));

            Operand IsOne(Operand Source) => context.Ceq(Source, 1);
            Operand IsZero(Operand Source) => context.Ceq(Source, 0);

            Operand Result = 0;

            switch ((Condition)((int)cond & ~1))
            {
                case Condition.EQ: Result = IsOne(Z); break;
                case Condition.CS: Result = IsOne(C); break;
                case Condition.MI: Result = IsOne(N); break;
                case Condition.VS: Result = IsOne(V); break;
                case Condition.HI: Result = context.And(IsOne(C) , IsZero(Z)); break;
                case Condition.GE: Result = context.Ceq(N, V); break;
                case Condition.GT: Result = context.And(context.Ceq(N, V), IsZero(Z)); break;
                case Condition.AL: Result = context.Const(1); break;
            }

            if ((((int)cond) & 1) == 1 && ((int)cond) != 0b1111)
            {
                Result = context.InvertBool(Result);
            }

            return Result;
        }

        public static void Csel(TranslationContext context) => Csel(context, false, false);
        public static void Csinc(TranslationContext context) => Csel(context, false, true);
        public static void Csinv(TranslationContext context) => Csel(context, true, false);
        public static void Csneg(TranslationContext context) => Csel(context, true, true);
        public static void Cbnz(TranslationContext context) => Cbz(context, true);
        public static void Cbz(TranslationContext context) => Cbz(context, false);
        public static void Tbnz(TranslationContext context) => Tbz(context, false);
        public static void Tbz(TranslationContext context) => Tbz(context, true);

        public static void Csel(TranslationContext context, bool not, bool inc)
        {
            context.SetSize("sf");

            Operand n = context.GetRegister("rn");
            Operand m = context.GetRegister("rm");

            Condition condition = (Condition)context.GetRaw("cond");

            EmitIf(context, ConditionHolds(context,condition),

                delegate()
                {
                    context.SetRegister("rd", n);
                },

                delegate()
                {
                    if (not)
                        m = context.Not(m);

                    if (inc)
                        m = context.Add(m,1);

                    context.SetRegister("rd", m);
                }             
                
                );
        }

        public static void Cbz(TranslationContext context, bool negate)
        {
            context.SetSize("sf");

            int imm = context.GetRaw("imm");
            Operand src = context.GetRegister("rt");

            Operand IsZero = context.Ceq(src, 0);

            if (negate)
                IsZero = context.InvertBool(IsZero);

            ulong NewAddress = context.CurrentOpCode.Address + (ulong)((((long)imm) << 50) >> 48);

            EmitIf(context, IsZero,

                delegate ()
                {
                    context.SetReturn(NewAddress);
                },

                delegate ()
                {
                    context.AdvancePC();
                }

                );
        }

        public static void Tbz(TranslationContext context, bool negate)
        {
            int b5 = context.GetRaw("b5");

            int b40 = context.GetRaw("b40");

            int imm = context.GetRaw("imm");

            Operand rt = context.GetRegister("rt");

            int bit = b40 | (b5 << 5);

            Operand test = context.And(context.ShiftRight(rt , bit) ,1);

            ulong NeweAddress = context.CurrentOpCode.Address + (SignExtendInt(imm, 14) << 2);

            if (negate)
                test = context.InvertBool(test);

            EmitIf(context, test,

                delegate ()
                {
                    context.SetReturn(NeweAddress);
                },

                delegate ()
                {
                    context.AdvancePC();
                }

                );
        }

        public static void Ccmm_Imm(TranslationContext context) => Ccm_Imm(context, true);
        public static void Ccmn_Reg(TranslationContext context) => Ccm(context, context.GetRegister("rn"), context.GetRegister("rm"), true);
        public static void Ccmp_Imm(TranslationContext context) => Ccm_Imm(context, false);
        public static void Ccmp_Reg(TranslationContext context) => Ccm(context, context.GetRegister("rn"), context.GetRegister("rm"), false);
        static void Ccm_Imm(TranslationContext context, bool negate) => Ccm(context, context.GetRegister("rn"), context.GetRaw("imm"), negate);

        public static void Ccm(TranslationContext context, Operand n, Operand m, bool negate)
        {
            context.SetSize("sf");

            int nzcv = context.GetRaw("nzcv");
            Condition cond = (Condition)context.GetRaw("cond");

            EmitIf(context, ConditionHolds(context, cond),

                delegate ()
                {
                    Operand d = 0;

                    if (negate)
                    {
                        d = context.Add(n , m);

                        SetAddsFlags(context, d, n, m);
                    }
                    else
                    {
                        d = context.Subtract(n , m);

                        SetSubsFlags(context, d, n, m);
                    }

                    CalculateNZ(context, d);
                },

                delegate ()
                {
                    SetFlagsImm(context, (ulong)nzcv);
                }

                );
        }
    }
}
