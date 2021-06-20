using GalacticARM.Decoding;
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
    public static partial class Emit64
    {
        public static bool EnableTracking = false;

        static unsafe Operand GetPhysicalAddress(TranslationContext context, Operand VirtualAddress, bool IsLoad)
        {
            Operand Index = context.ShiftRight(VirtualAddress,VirtualMemoryManager.PageBit);
            Operand Offset = context.And(VirtualAddress,VirtualMemoryManager.PageMask);

            if (sizeof(PageInfo) != 16)
            {
                throw new Exception();
            }

            Operand PageLoopUp = context.Add(context.GetRegRaw(nameof(ExecutionContext.MemoryPointer)),context.ShiftLeft(Index,4));

            Operand pa = context.Load64(PageLoopUp);

            if (EnableTracking) //This seems to be very slow :(
            {
                Operand PageDataPointer = context.Add(PageLoopUp, 8);

                Operand PageData = context.Load64(PageDataPointer);

                if (IsLoad)
                {
                    PageData = context.Or(PageData, 1);
                }
                else
                {
                    PageData = context.Or(PageData, 1 << 1);
                }

                context.Store64(PageDataPointer, PageData);
            }

            return context.Add(pa,Offset);
        }

        public static void Ldar(TranslationContext context) => Mem_Exclusive(context, true, false);
        public static void Ldaxr(TranslationContext context) => Mem_Exclusive(context, true, true);
        public static void Ldp_ImmIndexed(TranslationContext context) => MemP_ImmIndexed(context, true);
        public static void Ldpsw_ImmIndexed(TranslationContext context) => MemP_ImmIndexed(context, true, true, 2, 3);
        public static void Ldr_ImmIndexed(TranslationContext context) => Mem_ImmIndexed(context, true);
        public static void Ldr_Register(TranslationContext context) => Mem_Register(context, true);
        public static void Ldr_Unscaled(TranslationContext context) => Mem_UnscaledImm(context, true);
        public static void Ldrs_ImmIndexed(TranslationContext context) => Mem_ImmIndexed(context, true, true, context.GetRaw("size"), 3 - context.GetRaw("to"));
        public static void Ldrs_Register(TranslationContext context) => Mem_Register(context, true, true, context.GetRaw("size"), 3 - context.GetRaw("to"));
        public static void Ldrs_Unscaled(TranslationContext context) => Mem_UnscaledImm(context, true, true, context.GetRaw("size"), 3 - context.GetRaw("to"));
        public static void Ldxr(TranslationContext context) => Mem_Exclusive(context, true, true);
        public static void Stlr(TranslationContext context) => Mem_Exclusive(context, false, false);
        public static void Stlxr(TranslationContext context) => Mem_Exclusive(context, false, true);
        public static void Stp_ImmIndexed(TranslationContext context) => MemP_ImmIndexed(context, false);
        public static void Str_ImmIndexed(TranslationContext context) => Mem_ImmIndexed(context, false);
        public static void Str_Register(TranslationContext context) => Mem_Register(context, false);
        public static void Str_Unscaled(TranslationContext context) => Mem_UnscaledImm(context, false);
        public static void Stxr(TranslationContext context) => Mem_Exclusive(context, false, true);

        public static void Mem_ImmIndexed(TranslationContext context, bool IsLoad, bool SignExtend = false, int from = 0, int to = 0)
        {
            int imm = context.GetRaw("imm");

            int scale = context.GetRaw("size");

            ulong Imm = SignExtendInt(imm, 9);

            int Type = context.GetRaw("type");

            Operand Address = context.GetRegister("rn");

            if (Type == 1) //Post
            {
                context.SetRegister("rn", context.Add(Address , Imm));
            }
            else if (Type == 3) //Pre
            {
                Address = context.Add(Address,Imm);

                context.SetRegister("rn", Address);
            }
            else if (Type == 0)
            {
                Address = context.Add(Address, Imm);
            }
            else
            {
                context.ThrowUnknown();
            }

            Address = GetPhysicalAddress(context, Address,IsLoad);

            if (IsLoad)
            {
                Load(context, scale, Address, "rt", SignExtend, from, to);
            }
            else
            {
                Store(context, scale, Address, "rt");
            }
        }

        public static void Mem_UnscaledImm(TranslationContext context, bool IsLoad, bool SignExtend = false, int from = 0, int to = 0)
        {
            int imm = context.GetRaw("imm");

            int scale = context.GetRaw("size");

            imm <<= scale;

            Operand Address = context.Add(context.GetRegister("rn"), imm);

            //Console.WriteLine(Convert.ToString(context.CurrentOpCode.RawOpCode,2));

            Address = context.CreateLocal(GetPhysicalAddress(context, Address,IsLoad));

            if (IsLoad)
            {
                Load(context, scale, Address, "rt", SignExtend, from, to);
            }
            else
            {
                Store(context, scale, Address, "rt");
            }
        }

        public static void Mem_Register(TranslationContext context, bool IsLoad, bool SignExtend = false, int from = 0, int to = 0)
        {
            int scale = context.GetRaw("size");

            int s = context.GetRaw("s");

            IntType Type = (IntType)context.GetRaw("option");

            Operand Address = context.GetRegister("rn");

            Operand m = context.GetRegister("rm");

            m = context.ShiftLeft(Extend(context,m, Type) , (scale * s));

            Address = context.Add(Address, m);

            Address = context.CreateLocal(GetPhysicalAddress(context, Address,IsLoad));

            if (IsLoad)
            {
                Load(context, scale, Address, "rt", SignExtend, from, to);
            }
            else
            {
                Store(context, scale, Address, "rt");
            }
        }

        public static void MemP_ImmIndexed(TranslationContext context, bool IsLoad, bool SignExtend = false, int from = 0, int to = 0)
        {
            int opc = (context.CurrentOpCode.RawOpCode >> 30) & 0b11;

            int imm = context.GetRaw("imm");

            int Type = context.GetRaw("type");

            int scale = 2 + ((opc >> 1) & 1);

            ulong Imm = SignExtendInt(imm, 7) << scale;

            Operand Address = context.GetRegister("rn");

            if (Type == 2) //Offset
            {
                Address = context.Add(Address,Imm);
            }
            else if (Type == 3) //Pre
            {
                Address = context.Add(Address, Imm);

                context.SetRegister("rn", Address);
            }
            else if (Type == 1) //Post
            {
                context.SetRegister("rn", context.Add(Address,Imm));
            }
            else
            {
                context.ThrowUnknown();
            }

            Address = context.CreateLocal(GetPhysicalAddress(context, Address,IsLoad));

            if (IsLoad)
            {
                Load(context, scale, Address, "rt", SignExtend, from, to);
                Load(context, scale, context.Add(Address , ((8 << scale) >> 3)), "rt2", SignExtend, from, to);
            }
            else
            {
                Store(context, scale, Address, "rt");
                Store(context, scale, context.Add(Address , ((8 << scale) >> 3)), "rt2");
            }
        }

        public static void Load(TranslationContext context, int Scale, Operand Address, string name, bool SignExtend, int from, int to)
        {
            Operand load;

            switch (Scale)
            {
                case 0: load = context.Load8(Address); break;
                case 1: load = context.Load16(Address); break;
                case 2: load = context.Load32(Address); break;
                case 3: load = context.Load64(Address); break;
                default: load = context.ThrowUnknown(); break;
            }

            if (SignExtend)
            {
                switch (from)
                {
                    case 0: load = context.SignExtend8(load); break;
                    case 1: load = context.SignExtend16(load); break;
                    case 2: load = context.SignExtend32(load); break;
                }

                if (to == 2)
                {
                    load = context.And(load,uint.MaxValue);
                }
            }

            context.SetRegister(name,load);
        }

        public static void Store(TranslationContext context, int Scale, Operand Address, string name)
        {
            Operand ToStore = context.GetRegister(name);

            switch (Scale)
            {
                case 0: context.Store8(Address,ToStore); break;
                case 1: context.Store16(Address, ToStore); break;
                case 2: context.Store32(Address, ToStore); break;
                case 3: context.Store64(Address, ToStore); break;
                default: context.ThrowUnknown(); break;
            }
        }

        public static void Mem_Exclusive(TranslationContext context, bool IsLoad, bool Test)
        {
            //context.ReturnNil();

            //;

            int size = context.GetRaw("size");

            Operand Address = context.GetRegister("rn");

            Address = GetPhysicalAddress(context, Address,IsLoad);

            if (IsLoad)
            {
                Load(context, size, Address, "rt", false, 0, 0);

                if (Test)
                {
                    context.Call(nameof(FallbackMemory.SetExclusive_fb), context.ContextPointer(), Address);
                }
            }
            else
            {
                if (Test)
                {
                    Operand test = context.Call(nameof(FallbackMemory.TestExclusive_fb), context.ContextPointer(), Address);

                    EmitUniversal.EmitIf(context, test,

                        delegate ()
                        {
                            context.SetRegister("rs", 0);

                            Store(context, size, Address, "rt");

                            context.Call(nameof(FallbackMemory.Clrex_fb), context.ContextPointer());
                        },

                        delegate ()
                        {
                            context.SetRegister("rs", 1);
                        }


                        );
                }
                else
                {
                    Store(context, size, Address, "rt");
                }
            }
        }

        public static void Clrex(TranslationContext context)
        {
            //context.Call(nameof(FallbackMemory.Clrex_fb), context.ContextPointer());

            context.SetRegRaw(nameof(ExecutionContext.ExclusiveAddress),ulong.MaxValue);

            context.AdvancePC();
        }

    }
}
