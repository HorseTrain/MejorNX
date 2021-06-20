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
    public static partial class Emit64
    {
        public static void vec_Ldp_Imm(TranslationContext context) => vec_MemP_Imm(context, true);
        public static void vec_Ldr_Imm(TranslationContext context) => vec_Mem_Imm(context, true);
        public static void vec_Ldr_ImmIndexed(TranslationContext context) => vec_Mem_ImmIndexed(context, true);
        public static void vec_Stp_Imm(TranslationContext context) => vec_MemP_Imm(context, false);
        public static void vec_Str_Imm(TranslationContext context) => vec_Mem_Imm(context, false);
        public static void vec_Str_ImmIndexed(TranslationContext context) => vec_Mem_ImmIndexed(context, false);
        public static void vec_Ldr_Register(TranslationContext context) => vec_Mem_Register(context, true);
        public static void vec_Str_Register(TranslationContext context) => vec_Mem_Register(context, false);

        public static void vec_Mem_Register(TranslationContext context, bool IsLoad)
        {
            int opc = context.GetRaw("opc");

            int rt = context.GetRaw("rt");

            IntType Type = (IntType)context.GetRaw("option");

            int size = context.GetRaw("size");

            int s = context.GetRaw("s");

            int scale = (opc << 2) | size;

            Operand Address = context.GetRegister("rn");

            Operand m = context.GetRegister("rm");

            m = context.ShiftLeft(Extend(context,m, Type) , (scale * s));

            Address = context.Add(Address,m);

            Address = GetPhysicalAddress(context,Address,IsLoad);

            if (IsLoad)
            {
                Load(context, Address, rt, scale);
            }
            else
            {
                Store(context, Address, rt, scale);
            }
        }

        public static void vec_Mem_ImmIndexed(TranslationContext context, bool IsLoad)
        {
            int size = context.GetRaw("size");

            int opc = context.GetRaw("opc");

            int type = context.GetRaw("type");

            int imm = context.GetRaw("imm");

            int rt = context.GetRaw("rt");

            int scale = ((opc & 1) << 2) | size;

            ulong Imm = SignExtendInt(imm, 9);

            Operand Address = context.GetRegister("rn");

            if (type == 1) //Post
            {
                context.SetRegister("rn", context.Add(Address , Imm));
            }
            else if (type == 3) //Pre
            {
                Address = context.Add(Address,Imm);

                context.SetRegister("rn", Address);
            }
            else if (type == 0)
            {
                Address = context.Add(Address, Imm);
            }
            else
            {
                context.ThrowUnknown();
            }

            Address = GetPhysicalAddress(context,Address,IsLoad);

            if (IsLoad)
            {
                Load(context, Address, rt, scale);
            }
            else
            {
                Store(context, Address, rt, scale);
            }
        }

        public static void vec_Mem_Imm(TranslationContext context, bool IsLoad)
        {
            int opc = context.GetRaw("opc");

            int size = context.GetRaw("size");

            int scale = (opc << 2) | size;

            int imm = context.GetRaw("imm");

            Operand Address = context.GetRegister("rn");

            Address = context.Add(Address,(imm << scale));

            int rt = context.GetRaw("rt");

            Address = GetPhysicalAddress(context,Address,IsLoad);

            if (IsLoad)
            {
                Load(context, Address, rt, scale);
            }
            else
            {
                Store(context, Address, rt, scale);
            }
        }

        public static void vec_MemP_Imm(TranslationContext context, bool IsLoad)
        {
            int opc = context.GetRaw("opc");

            int imm = context.GetRaw("imm");

            int type = context.GetRaw("type");

            int scale = 2 + opc;

            int rt = context.GetRaw("rt");
            int rt2 = context.GetRaw("rt2");

            ulong Imm = SignExtendInt(imm, 7) << scale;

            Operand Address = context.GetRegister("rn");

            if (type == 2) //imm
            {
                Address = context.Add(Address,Imm);
            }
            else if (type == 3) //Pre
            {
                Address = context.Add(Address, Imm);

                context.SetRegister("rn", Address);
            }
            else if (type == 1) //Post
            {
                context.SetRegister("rn", context.Add(Address,Imm));
            }
            else
            {
                context.ThrowUnknown();
            }

            Address = GetPhysicalAddress(context,Address,IsLoad);

            if (IsLoad)
            {
                Load(context, Address, rt, scale);
                Load(context, context.Add(Address , (1 << scale)), rt2, scale);
            }
            else
            {
                Store(context, Address, rt, scale);
                Store(context, context.Add(Address, (1 << scale)), rt2, scale);
            }
        }

        public static void Load(TranslationContext context, Operand Address, int rt, int size)
        {
            if (size == 4)
            {
                context.SetVector(rt, context.LoadVector(Address, size));
            }
            else
            {
                Operand load;

                switch (size)
                {
                    case 0: load = context.Load8(Address); break;
                    case 1: load = context.Load16(Address); break;
                    case 2: load = context.Load32(Address); break;
                    case 3: load = context.Load64(Address); break;
                    default: load = context.ThrowUnknown(); break;
                }

                Operand vec = context.CreateVector();

                context.SetVectorElement(vec,load,0,size);

                context.SetVector(rt,vec);
            }
        }

        public static void Store(TranslationContext context, Operand Address, int rt, int size)
        {
            if (size == 4)
            {
                context.StoreVector(context.GetVector(rt), Address, size);
            }
            else
            {
                Operand vec = context.GetVector(rt);

                Operand store = context.GetVectorElement(vec,0,3);

                switch (size)
                {
                    case 0: context.Store8(Address, store); break;
                    case 1: context.Store16(Address, store); break;
                    case 2: context.Store32(Address, store); break;
                    case 3: context.Store64(Address, store); break;
                    default: throw new NotImplementedException();
                }
            }
        }

        static object Lock = new object();

        public static void Ld1r(TranslationContext context)
        {
            Operand n = context.GetRegister("rn");
            int rt = context.GetRaw("rt");
            int size = context.GetRaw("size");
            int q = context.GetRaw("q");

            Operand load = context.Load64(GetPhysicalAddress(context,n,true));

            Operand res = context.CreateVector();

            for (int i = 0; i < 16 >> size; i++)
            {
                context.SetVectorElement(res,load,i,size);
            }

            if (q == 0)
            {
                context.SetVectorElement(res,0,1,3);
            }

            context.SetVector(rt,res);
        }
    }
}
