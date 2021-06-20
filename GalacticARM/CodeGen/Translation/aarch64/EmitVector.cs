using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime.Fallbacks;
using System;

namespace GalacticARM.CodeGen.Translation.aarch64
{
    public static partial class Emit64
    {
        public static int GetVectorSize(int imm5)
        {
            for (int i = 0; i < 4; i++)
            {
                if ((imm5 & ((1 << (i + 1)) - 1)) == 1 << i)
                {
                    return i;
                }
            }

            throw new NotImplementedException();
        }

        public static ulong BuildScaledInt(int size, params int[] args)
        {
            size = (1 << size) - 1;

            ulong Out = 0;

            for (int i = 0; i < args.Length; i++)
            {
                Out |= ((ulong)(args[args.Length - i - 1] * size)) << (i << 3);
            }

            return Out;
        }

        public static ulong BuildScaledInt32(int size, params int[] args)
        {
            size = (1 << size) - 1;

            ulong Out = 0;

            for (int i = 0; i < args.Length; i++)
            {
                Out |= ((ulong)(args[args.Length - i - 1] * size)) << (i << 2);
            }

            return Out;
        }

        static void ClearVectorTop(TranslationContext context, Operand vector) => context.SetVectorElement(vector, 0, 1, 3, false);

        public static void Dup_General(TranslationContext context)
        {
            Operand n = context.GetRegister("rn");
            int rd = context.GetRaw("rd");
            int q = context.GetRaw("q");

            int imm = context.GetRaw("imm");

            int size = GetVectorSize(imm);

            Operand d = context.CreateVector();

            int iter = (16 >> size);

            for (int i = 0; i < iter >> (1 - q); i++)
            {
                context.SetVectorElement(d, n, i, size);
            }

            context.SetVector(rd, d);
        }

        public static void Movi(TranslationContext context)
        {
            //EmitUniversal.EmitUnicornFB(context);

            //return;
            int opCode = context.CurrentOpCode.RawOpCode;

            int Rd = opCode & 0x1f;

            int Size = 0;

            int cMode = (opCode >> 12) & 0xf;
            int op = (opCode >> 29) & 0x1;

            int modeLow = cMode & 1;
            int modeHigh = cMode >> 1;

            long imm;

            imm = ((uint)opCode >> 5) & 0x1f;
            imm |= ((uint)opCode >> 11) & 0xe0;

            long Immediate;

            long ShlOnes(long value, int shift)
            {
                if (shift != 0)
                {
                    return value << shift | (long)(ulong.MaxValue >> (64 - shift));
                }
                else
                {
                    return value;
                }
            }

            if (modeHigh == 0b111)
            {
                Size = modeLow != 0 ? op : 3;

                switch (op | (modeLow << 1))
                {
                    case 0:
                        // 64-bits Immediate.
                        // Transform abcd efgh into abcd efgh abcd efgh ...
                        imm = (long)((ulong)imm * 0x0101010101010101);
                        break;

                    case 1:
                        // 64-bits Immediate.
                        // Transform abcd efgh into aaaa aaaa bbbb bbbb ...
                        imm = (imm & 0xf0) >> 4 | (imm & 0x0f) << 4;
                        imm = (imm & 0xcc) >> 2 | (imm & 0x33) << 2;
                        imm = (imm & 0xaa) >> 1 | (imm & 0x55) << 1;

                        imm = (long)((ulong)imm * 0x8040201008040201);
                        imm = (long)((ulong)imm & 0x8080808080808080);

                        imm |= imm >> 4;
                        imm |= imm >> 2;
                        imm |= imm >> 1;
                        break;

                    case 2:
                    case 3:
                        // Floating point Immediate.
                        imm = DecoderHelper.DecodeImm8Float(imm, Size);
                        break;
                }
            }
            else if ((modeHigh & 0b110) == 0b100)
            {
                // 16-bits shifted Immediate.
                Size = 1; imm <<= (modeHigh & 1) << 3;
            }
            else if ((modeHigh & 0b100) == 0b000)
            {
                // 32-bits shifted Immediate.
                Size = 2; imm <<= modeHigh << 3;
            }
            else if ((modeHigh & 0b111) == 0b110)
            {
                // 32-bits shifted Immediate (fill with ones).
                Size = 2; imm = ShlOnes(imm, 8 << modeLow);
            }
            else
            {
                // 8 bits without shift.
                Size = 0;
            }

            Immediate = imm;

            //RegisterSize = ((opCode >> 30) & 1) != 0
            //    ? RegisterSize.Simd128
            //    : RegisterSize.Simd64;

            int cmode = context.GetRaw("cmode");
            //int op = context.GetRaw("op");
            int q = context.GetRaw("q");

            int a = context.GetRaw("a");
            int b = context.GetRaw("b");
            int c = context.GetRaw("c");
            int d = context.GetRaw("d");
            int e = context.GetRaw("e");
            int f = context.GetRaw("f");
            int g = context.GetRaw("g");
            int h = context.GetRaw("h");

            int rd = context.GetRaw("rd");

            int hi = ((context.CurrentOpCode.RawOpCode >> 16) & 0x7);
            int low = ((context.CurrentOpCode.RawOpCode >> 5) & 0x1F);

            ulong RawIMM = (ulong)((hi << 5) | low);

            Operand result = context.CreateVector();

            int size = 0;

            if (op == 0 && cmode == 0b1110)
            {
                size = 0;
            }
            else if (op == 0 && (cmode & 0b1101) == 0b1000)
            {
                size = 1;
            }
            else if (op == 0 && (cmode & 0b1001) == 0)
            {
                size = 2;
            }
            else if (op == 0 && (cmode & 0b1110) == 0b1100)
            {
                size = 2;
            }
            else if (q == 0 && op == 1 && (cmode == 0b1110))
            {
                size = 3;
            }
            else if (q == 1 && op == 1 && cmode == 0b1110)
            {
                size = 3;
            }
            else
            {
                EmitUniversal.EmitUnicornFB(context);

                return;

                //context.ThrowUnknown();
            }

            for (int i = 0; i < 16 >> size; i++)
            {
                context.SetVectorElement(result,Immediate,i,size);
            }

            if (q == 0)
            {
                context.SetVectorElement(result,0,1,3);
            }

            context.SetVector(rd, result);
        }

        public static (int, int, int) GetElements(int imm5, int imm4)
        {
            int Size = imm5 & -imm5;

            switch (Size)
            {
                case 1: Size = 0; break;
                case 2: Size = 1; break;
                case 4: Size = 2; break;
                case 8: Size = 3; break;
            }

            int SrcIndex = imm4 >> Size;
            int DstIndex = imm5 >> (Size + 1);

            return (SrcIndex, DstIndex, Size);
        }

        public static void Ins_Element(TranslationContext context)
        {
            int imm5 = context.GetRaw("imm5");

            int imm4 = context.GetRaw("imm4");

            (int Src, int Des, int Size) = GetElements(imm5, imm4);

            int rn = context.GetRaw("rn");

            int rd = context.GetRaw("rd");

            Operand des = context.GetVector(rd);
            Operand src = context.GetVector(rn);

            context.SetVectorElement(des, context.GetVectorElement(src, Src, Size), Des, Size);

            context.SetVector(rd, des);
        }

        public static void Ins_General(TranslationContext context)
        {
            int imm = context.GetRaw("imm");

            int size = GetVectorSize(imm);

            int rd = context.GetRaw("rd");

            int index = imm >> (size + 1);

            Operand n = context.GetRegister("rn");

            Operand des = context.GetVector(rd);

            context.SetVectorElement(des, n, index, size);

            context.SetVector(rd, des);
        }

        public static void Orr_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Or, false);
        public static void Orn_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Or, true);

        public static void Eor_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Xor, false);
        public static void Eon_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_Xor, true);

        public static void And_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_And, false);
        public static void Bic_Vector(TranslationContext context) => VectorOperation(context, Instruction.Vector_And, true);

        public static void Add_Vector(TranslationContext context) => VectoOperationRespectSize(context, Instruction.Add);

        public static void VectorOperation(TranslationContext context, Instruction instruction, bool not)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            int q = context.GetRaw("q");

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);

            if (not)
                context.VectorNot(m);

            Operand d = context.VectorOperation(n, m, instruction, q == 0);

            context.SetVector(rd, d);
        }

        public static void VectoOperationRespectSize(TranslationContext context, Instruction instruction)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            int q = context.GetRaw("q");

            int size = context.GetRaw("size");

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);


        }

        public static void Cnt(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int q = context.GetRaw("q");

            context.Call(nameof(Fallbackbits.Cnt), context.ContextPointer(), rd, rn);

            if (q == 0)
            {
                context.SetVectorElement(Operand.Vec(rd), 0, 1, 3);
            }
        }

        public static void Uaddlv(TranslationContext context)
        {
            int q = context.GetRaw("q");

            int rn = context.GetRaw("rn");
            int rd = context.GetRaw("rd");

            int size = context.GetRaw("size");

            int Size = 64 << q;

            int elms = (Size >> 3) >> Size;

            Operand src = context.GetVector(rn);
            Operand des = context.CreateVector();

            Operand res = context.GetVectorElement(src, 0, size);

            for (int i = 1; i < elms; i++)
            {
                res = context.Add(res, context.GetVectorElement(src, i, size));
            }

            context.SetVectorElement(des, res, 0, 3);

            context.SetVector(rd, des);
        }

        public static void Neg_Vector(TranslationContext context)
        {
            int q = context.GetRaw("q");

            int rn = context.GetRaw("rn");
            int rd = context.GetRaw("rd");

            int size = context.GetRaw("size");

            int iterations = 16 >> size;

            Operand src = context.GetVector(rn);

            Operand d = context.CreateVector();

            for (int i = 0; i < iterations; i++)
            {
                Operand op = context.GetVectorElement(src, i, size);

                context.SetVectorElement(d, context.Add(context.Not(op), 1), i, size);
            }

            if (q == 0)
            {
                context.SetVectorElement(d, 0, 1, 3);
            }

            context.SetVector(rd, d);
        }

        public static void Ext_Vector(TranslationContext context)
        {
            int q = context.GetRaw("q");

            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");
            int rd = context.GetRaw("rd");

            int imm = context.GetRaw("imm");

            //Operand n = context.GetVector(rn);
            //Operand m = context.GetVector(rm);

            Operand res = context.CreateVector();

            int bytes = q == 1 ? 16 : 8;

            int position = imm & (bytes - 1);

            for (int index = 0; index < bytes; index++)
            {
                int reg = imm + index < bytes ? rn : rm;

                Operand e = context.GetVectorElement(Operand.Vec(reg), position, 0);

                position = (position + 1) & (bytes - 1);

                context.SetVectorElement(res, e, index, 0);
            }

            context.SetVector(rd, res);
        }

        public static void Sshll(TranslationContext context) => Shll(context, 0, true);
        public static void Ushll(TranslationContext context) => Shll(context, 0, false);

        public static void Shll(TranslationContext context, int ins, bool singed)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int immb = context.GetRaw("immb");
            int immh = context.GetRaw("immh");

            if (immh == 0)
            {
                context.ThrowUnknown();
            }

            int q = context.GetRaw("q");

            int Imm = (context.CurrentOpCode.RawOpCode >> 16) & 0x7f;

            int Size = BitUtils.HighestBitSetNibble(Imm >> 3);

            int elems = 8 >> Size;

            int part = q == 1 ? elems : 0;

            Operand n = context.GetVector(rn);
            Operand res = context.CreateVector();

            int shift = Imm - (8 << Size);

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.GetVectorElement(n, part + index, Size, singed);

                switch (ins)
                {
                    case 0: context.SetVectorElement(res, context.ShiftLeft(ne, shift), index, Size + 1); break;
                    default: context.ThrowUnknown(); break;
                }
            }

            context.SetVector(rd, res);
        }

        public static void Dup_ElementScalar(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int imm5 = context.GetRaw("imm");

            int Size = GetVectorSize(imm5);

            int index = (imm5 >> (Size + 1));

            Operand n = context.GetVector(rn);
            Operand d = context.CreateVector();

            context.SetVectorElement(d, context.GetVectorElement(n, index, Size), 0, Size);

            context.SetVector(rd, d);
        }

        public static void Dup_ElementVector(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int imm5 = context.GetRaw("imm");

            int Size = GetVectorSize(imm5);

            int q = context.GetRaw("q");

            int index = (imm5 >> (Size + 1));

            Operand n = context.GetVector(rn);
            Operand d = context.CreateVector();

            int iterations = 16 >> Size;

            Operand src = context.GetVectorElement(n, index, Size);

            for (int i = 0; i < iterations; i++)
            {
                context.SetVectorElement(d, src, i, Size);
            }

            if (q == 0)
            {
                context.SetVectorElement(d, 0, 1, 3);
            }

            context.SetVector(rd, d);
        }

        public static void Xtn(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int size = context.GetRaw("size");

            int q = context.GetRaw("q");

            int elems = 8 >> size;

            int part = q == 1 ? elems : 0;

            Operand n = context.GetVector(rn);

            Operand d = part == 0 ? context.CreateVector() : context.GetVector(rd);

            for (int i = 0; i < elems; i++)
            {
                Operand ne = context.GetVectorElement(n, i, size + 1);

                context.SetVectorElement(d, ne, part + i, size);
            }

            context.SetVector(rd, d);
        }

        public static void Shl(TranslationContext context) => VectorShift(context, 0);
        public static void Sshr(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int immb = context.GetRaw("immb");
            int immh = context.GetRaw("immh");

            if (immh == 0)
            {
                context.ThrowUnknown();
            }

            int q = context.GetRaw("q");

            int Imm = (context.CurrentOpCode.RawOpCode >> 16) & 0x7f;

            int Size = BitUtils.HighestBitSetNibble(Imm >> 3);

            int esize = 8 << Size;

            int datasize = q == 1 ? 128 : 64;

            int elements = datasize / esize;

            int shift = (esize * 2) - Imm;

            Operand n = context.GetVector(rn);
            Operand res = context.CreateVector();

            for (int i = 0; i < elements; i++)
            {
                Operand ne = context.GetVectorElement(n, i, Size, true);

                ne = context.ShiftRight_Singed(ne, shift);

                context.SetVectorElement(res, ne, i, Size);
            }

            context.SetVector(rd, res);
        }

        public static void VectorShift(TranslationContext context, int ins, bool singed = false)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int immb = context.GetRaw("immb");
            int immh = context.GetRaw("immh");

            if (immh == 0)
            {
                context.ThrowUnknown();
            }

            int q = context.GetRaw("q");

            int Imm = (context.CurrentOpCode.RawOpCode >> 16) & 0x7f;

            int Size = BitUtils.HighestBitSetNibble(Imm >> 3);

            int shift = Imm - (8 << Size);

            int elems = (q == 1 ? 16 : 8) >> Size;

            Operand n = context.GetVector(rn);
            Operand d = context.CreateVector();

            context.CurrentSize = IntSize.Int64;

            for (int i = 0; i < elems; i++)
            {
                Operand ne = context.GetVectorElement(n, i, Size, singed);

                switch (ins)
                {
                    case 0: context.SetVectorElement(d, context.ShiftLeft(ne, shift), i, Size); break;
                    default: context.ThrowUnknown(); break;
                }
            }

            context.SetVector(rd, d);
        }

        public static void Zip(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            int size = context.GetRaw("size");

            int q = context.GetRaw("q");

            int esize = 8 << size;
            int datasize = q == 1 ? 128 : 64;
            int elements = datasize / esize;
            int pairs = elements / 2;
            int part = context.GetRaw("op");

            Operand n = context.GetVector(rn);
            Operand m = context.GetVector(rm);

            Operand d = context.CreateVector();

            int Base = part * pairs;

            for (int p = 0; p < pairs; p++)
            {
                context.SetVectorElement(d, context.GetVectorElement(n, Base + p, size), 2 * p, size);
                context.SetVectorElement(d, context.GetVectorElement(m, Base + p, size), 2 * p + 1, size);
            }

            context.SetVector(rd, d);
        }

        public static void Bsl(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");
            int rm = context.GetRaw("rm");

            int q = context.GetRaw("q");

            Operand m = context.GetVector(rm);
            Operand d = context.GetVector(rd);
            Operand n = context.GetVector(rn);

            Operand res = context.VectorOperation(m, context.VectorOperation(context.VectorOperation(m, n, Instruction.Vector_Xor), d, Instruction.Vector_And), Instruction.Vector_Xor);

            if (q == 0)
            {
                context.SetVectorElement(res, 0, 1, 3);
            }

            context.SetVector(rd, res);
        }

        public static void Umov_ToGeneral(TranslationContext context)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int imm = context.GetRaw("imm");

            int Size = GetVectorSize(imm);

            int index = imm >> (Size + 1);

            Operand n = context.GetVectorElement(Operand.Vec(rn), index, Size);

            context.SetRegister("rd", n);
        }

        public static void Frintp_Scalar(TranslationContext context) => Frint_Scalar(context, 0);
        public static void Frintm_Scalar(TranslationContext context) => Frint_Scalar(context, 1);

        public static void Frint_Scalar(TranslationContext context, int mode)
        {
            int rd = context.GetRaw("rd");
            int rn = context.GetRaw("rn");

            int ftype = context.GetRaw("ftype");

            if (ftype == 3)
            {
                context.ThrowUnknown();
            }

            int size = ftype + 2;

            Operand n = context.GetVector(rn);

            Operand d = context.Call(nameof(FallbackFloat.FloorCel), context.GetVectorElement(n, 0, size), size, mode);

            Operand res = context.CreateVector();

            context.SetVectorElement(res, d, 0, size);

            context.SetVector(rd, res);
        }
    }
}
