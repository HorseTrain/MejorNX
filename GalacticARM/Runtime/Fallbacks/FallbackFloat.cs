using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static unsafe class FallbackFloat
    {
        public static double Floor64(double source) => Math.Floor(source);
        public static double Ceiling64(double source) => Math.Ceiling(source);

        public static uint ConvertFloatToUint(float Source)
        {
            return (*(uint*)&Source);
        }

        public static ulong ConvertDoubleToUlong(double Source)
        {
            return (*(ulong*)&Source);
        }

        public static float ConvertUintToFloat(uint Source)
        {
            return *(float*)&Source;
        }

        public static double ConvertUlongToDouble(ulong Source)
        {
            return *(double*)&Source;
        }

        public static void FB_Fcvtz_Scalar_Fixed(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            int rd = (int)context->Arg0;
            ulong src = context->Arg1;
            int ftype = (int)context->Arg2;
            int sf = (int)context->Arg3;
            bool singed = context->Arg4 == 1 ? true : false;
            bool towardzero = context->Arg5 == 1 ? true : false;

            ulong des = 0;

            if (singed)
            {
                if (towardzero)
                {
                    if (ftype == 2)
                    {
                        float flt = ConvertUintToFloat((uint)src);

                        if (sf == 2)
                        {
                            des = (uint)SatF32ToS32(flt);
                        }
                        else if (sf == 3)
                        {
                            des = (ulong)SatF32ToS64(flt);
                        }
                    }
                    else if (ftype == 3)
                    {
                        double flt = ConvertUlongToDouble(src);

                        if (sf == 2)
                        {
                            des = (uint)SatF64ToS32(flt);
                        }
                        else if (sf == 3)
                        {
                            des = (ulong)SatF64ToS64(flt);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }   
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (towardzero)
                {
                    if (ftype == 2)
                    {
                        float flt = ConvertUintToFloat((uint)src);

                        if (sf == 2)
                        {
                            des = SatF32ToU32(flt);
                        }
                        else if (sf == 3)
                        {
                            des = SatF32ToU64(flt);
                        }
                    }
                    else if (ftype == 3)
                    {
                        double flt = ConvertUlongToDouble(src);

                        if (sf == 2)
                        {
                            des = SatF64ToU32(flt);
                        }
                        else if (sf == 3)
                        {
                            des = SatF64ToU64(flt);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            context->SetX(rd,des);
        }

        public static ulong FloorCel(ulong src, ulong size, ulong mode)
        {            
            if (size == 3)
            {
                double d = ConvertUlongToDouble(src);

                if (mode == 1)
                {
                    d = Math.Floor(d);
                }
                else if (mode == 0)
                {
                    d = Math.Ceiling(d);
                }
                else
                {
                    throw new NotImplementedException();
                }

                src = ConvertDoubleToUlong(d);
            }
            else if (size == 2)
            {
                float d = ConvertUintToFloat((uint)src);

                if (mode == 1)
                {
                    d = MathF.Floor(d);
                }
                else if (mode == 0)
                {
                    d = MathF.Ceiling(d);
                }
                else
                {
                    throw new NotImplementedException();
                }

                src = ConvertFloatToUint(d);
            }
            else
            {
                throw new NotImplementedException();
            }

            return src;
        }

        public static int SatF32ToS32(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= int.MaxValue ? int.MaxValue :
                   value <= int.MinValue ? int.MinValue : (int)value;
        }
        public static long SatF32ToS64(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= long.MaxValue ? long.MaxValue :
                   value <= long.MinValue ? long.MinValue : (long)value;
        }
        public static uint SatF32ToU32(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= uint.MaxValue ? uint.MaxValue :
                   value <= uint.MinValue ? uint.MinValue : (uint)value;
        }
        public static ulong SatF32ToU64(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= ulong.MaxValue ? ulong.MaxValue :
                   value <= ulong.MinValue ? ulong.MinValue : (ulong)value;
        }
        public static int SatF64ToS32(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= int.MaxValue ? int.MaxValue :
                   value <= int.MinValue ? int.MinValue : (int)value;
        }
        public static long SatF64ToS64(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= long.MaxValue ? long.MaxValue :
                   value <= long.MinValue ? long.MinValue : (long)value;
        }
        public static uint SatF64ToU32(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= uint.MaxValue ? uint.MaxValue :
                   value <= uint.MinValue ? uint.MinValue : (uint)value;
        }
        public static ulong SatF64ToU64(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= ulong.MaxValue ? ulong.MaxValue :
                   value <= ulong.MinValue ? ulong.MinValue : (ulong)value;
        }

        public static void ConvertPerc(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            int rd = (int)context->Arg0;
            int rn = (int)context->Arg1;
            int ftype = (int)context->Arg2;
            int opc = (int)context->Arg3;

            if (ftype == 2 && opc == 3)
            {
                //float -> double

                float src = context->GetQ(rn).GetElement(0);

                double des = (double)src;

                context->SetQ(rd,new Vector128<double>().WithElement(0,des).AsSingle());
            }
            else if (ftype == 3 && opc == 2)
            {
                //double -> float

                double src = context->GetQ(rn).AsDouble().GetElement(0);

                float des = (float)src;

                context->SetQ(rd,new Vector128<float>().WithElement(0,des));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void Fsqrt(ulong _context, ulong rd, ulong rn, ulong size)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            if (size == 2)
            {
                float source = context->GetQ((int)rn).GetElement(0);

                source = MathF.Sqrt(source);

                context->SetQ((int)rd, new Vector128<float>().WithElement(0, source));
            }
            else if (size == 3)
            {
                double source = context->GetQ((int)rn).AsDouble().GetElement(0);

                source = Math.Sqrt(source);

                context->SetQ((int)rd, new Vector128<double>().WithElement(0, source).AsSingle());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static unsafe void Fcmp(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            int rn = (int)context->Arg0;
            int rm = (int)context->Arg1;
            int size = (int)context->Arg2;
            bool WithZero = context->Arg3 == 1;

            if (size == 2)
            {
                float n = context->GetQ(rn).GetElement(0);
                float m = context->GetQ(rm).GetElement(0);

                if (WithZero)
                    m = 0;

                if (float.IsNaN(n) || float.IsNaN(m))
                {
                    context->SetFlagsImm(0b0011);
                }
                else
                {
                    if (n == m)
                    {
                        context->SetFlagsImm(0b0110);
                    }
                    else if (n < m)
                    {
                        context->SetFlagsImm(0b1000);
                    }
                    else //n > m
                    {
                        context->SetFlagsImm(0b0010);
                    }
                }
            }
            else if (size == 3)
            {
                double n = context->GetQ(rn).AsDouble().GetElement(0);
                double m = context->GetQ(rm).AsDouble().GetElement(0);

                if (WithZero)
                    m = 0;

                if (double.IsNaN(n) || double.IsNaN(m))
                {
                    context->SetFlagsImm(0b0011);
                }
                else
                {
                    if (n == m)
                    {
                        context->SetFlagsImm(0b0110);
                    }
                    else if (n < m)
                    {
                        context->SetFlagsImm(0b1000);
                    }
                    else //n > m
                    {
                        context->SetFlagsImm(0b0010);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static ulong FCompare(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            int rn = (int)context->Arg0;
            int rm = (int)context->Arg1;
            ulong ismax = context->Arg2;
            ulong type = context->Arg3;

            if (type == 2)
            {
                float n = context->GetQ(rn).GetElement(0);
                float m = context->GetQ(rm).GetElement(0);

                if (ismax == 1)
                    return n > m ? 1UL : 0;

                return n < m ? 1UL : 0;
            }
            else if (type == 3)
            {
                double n = context->GetQ(rn).AsDouble().GetElement(0);
                double m = context->GetQ(rm).AsDouble().GetElement(0);

                if (ismax == 1)
                    return n > m ? 1UL : 0;

                return n < m ? 1UL : 0;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void UnsingedToFloat(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            int des = (int)context->Arg0;
            int src = (int)context->Arg1;
            int from = (int)context->Arg2;
            int to = (int)context->Arg3;

            ulong srcc = ((ulong*)context)[src];
            Vector128<float>* vdes = (Vector128<float>*)((byte*)context + ExecutionContext.VectorOffset);

            if (from == 2)
            {
                srcc &= uint.MaxValue;
            }

            if (to == 2)
            {
                float dess = (float)srcc;

                vdes[des] = new Vector128<float>().WithElement(0,dess);
            }
            else if (to == 3)
            {
                double dess = (double)srcc;

                vdes[des] = new Vector128<double>().WithElement(0, dess).AsSingle();
            }
        }
    }
}
