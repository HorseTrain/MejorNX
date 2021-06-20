using System;
using System.Runtime.Intrinsics;
using UltimateOrb;

namespace GalacticARM.Runtime.Fallbacks
{
    public static class Fallbackbits
    {
        public static ulong CountLeadingZeros(ulong Source, ulong Size)
        {
            ulong Out = 0;

            for (int i = (int)Size - 1; i >= 0; i--)
            {
                if (((Source >> i) & 1) == 0)
                {
                    Out++;
                }
                else
                {
                    break;
                }
            }

            return Out;
        }

        public static ulong MulH(ulong n, ulong m, ulong signed)
        {
            if (signed == 1)
            {
                Int128 nn = (long)n;
                Int128 mm = (long)m;

                Int128 dd = nn * mm;

                return (ulong)((dd >> 64) & ulong.MaxValue);
            }
            else
            {
                ulong Top = 0;
                ulong Bot = 0;

                mult64to128(n, m, ref Top, ref Bot);

                return Top;
            }
        }

        static void mult64to128(ulong u, ulong v, ref ulong h, ref ulong l)
        {
            ulong u1 = (u & 0xffffffff);
            ulong v1 = (v & 0xffffffff);
            ulong t = (u1 * v1);
            ulong w3 = (t & 0xffffffff);
            ulong k = (t >> 32);

            u >>= 32;
            t = (u * v1) + k;
            k = (t & 0xffffffff);
            ulong w1 = (t >> 32);

            v >>= 32;
            t = (u1 * v) + k;
            k = (t >> 32);

            h = (u * v) + w1 + k;
            l = (t << 32) + w3;
        }

        public static int CountBits(byte b)
        {
            int res = 0;

            for (int i = 0; i < 8; i++)
            {
                res += ((b >> i) & 1);
            }

            return res;
        }

        public static unsafe void Cnt(ulong _context, ulong rd, ulong rn)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            //Look into more.
            Vector128<byte> Source = context->GetQ((int)rn).AsByte();

            Vector128<byte> res = new Vector128<byte>();

            for (int i = 0; i < 16; i++)
            {
                int bitcount = CountBits((Source).GetElement(i));

                res = res.WithElement(i, (byte)bitcount);
            }

            context->SetQ((int)rd, res.AsSingle());
        }

        public static ulong Rev(ulong value, ulong size)
        {
            ulong Out = 0;

            if (size == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Out |= ((value >> (i * 8)) & 255) << ((3 - i) * 8);
                }
            }
            else if (size == 1)
            {
                for (int i = 0; i < 8; i++)
                {
                    Out |= ((value >> (i * 8)) & 255) << ((7 - i) * 8);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return Out;
        }
    }
}
