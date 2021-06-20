using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace GalacticARM.Runtime
{
    public unsafe struct LocalStore
    {
        public fixed ulong Buffer[2048 * 100];
    }

    public unsafe struct ExecutionContext
    {
        //Normal
        public ulong X0;
        public ulong X1;
        public ulong X2;
        public ulong X3;
        public ulong X4;
        public ulong X5;
        public ulong X6;
        public ulong X7;
        public ulong X8;
        public ulong X9;
        public ulong X10;
        public ulong X11;
        public ulong X12;
        public ulong X13;
        public ulong X14;
        public ulong X15;
        public ulong X16;
        public ulong X17;
        public ulong X18;
        public ulong X19;
        public ulong X20;
        public ulong X21;
        public ulong X22;
        public ulong X23;
        public ulong X24;
        public ulong X25;
        public ulong X26;
        public ulong X27;
        public ulong X28;
        public ulong X29;
        public ulong X30;
        public ulong X31;

        public ulong N;
        public ulong Z;
        public ulong C;
        public ulong V;

        public ulong FunctionPointer;
        public ulong Arg0;
        public ulong Arg1;
        public ulong Arg2;
        public ulong Arg3;
        public ulong Arg4;
        public ulong Arg5;
        public ulong Arg6;

        public ulong CallArgument;
        public ulong DebugHook;
        public ulong ID;

        public ulong IsExecuting;
        public ulong MemoryPointer;
        public ulong dczid_el0;
        public ulong fpcr;
        public ulong fpsr;
        public ulong tpidr;
        public ulong tpidrro_el0;
        public ulong MyPointer;

        public ulong ExecutedInstructions;
        public ulong Return;
        public ulong ExclusiveAddress;
        public ulong ExclusiveValue;

        public ulong FunctionTablePointer;

        LocalStore Locals;

        //Vector
        public Vector128<float> Q0;
        public Vector128<float> Q1;
        public Vector128<float> Q2;
        public Vector128<float> Q3;
        public Vector128<float> Q4;
        public Vector128<float> Q5;
        public Vector128<float> Q6;
        public Vector128<float> Q7;
        public Vector128<float> Q8;
        public Vector128<float> Q9;
        public Vector128<float> Q10;
        public Vector128<float> Q11;
        public Vector128<float> Q12;
        public Vector128<float> Q13;
        public Vector128<float> Q14;
        public Vector128<float> Q15;
        public Vector128<float> Q16;
        public Vector128<float> Q17;
        public Vector128<float> Q18;
        public Vector128<float> Q19;
        public Vector128<float> Q20;
        public Vector128<float> Q21;
        public Vector128<float> Q22;
        public Vector128<float> Q23;
        public Vector128<float> Q24;
        public Vector128<float> Q25;
        public Vector128<float> Q26;
        public Vector128<float> Q27;
        public Vector128<float> Q28;
        public Vector128<float> Q29;
        public Vector128<float> Q30;
        public Vector128<float> Q31;

        LocalStore VectorLocals;

        public void SetFlagsImm(ulong imm)
        {
            N = (imm >> 3) & 1;
            Z = (imm >> 2) & 1;
            C = (imm >> 1) & 1;
            V = (imm >> 0) & 1;
        }

        public ulong NZCV
        {
            set
            {
                SetFlagsImm(value >> 28);
            }

            get
            {
                ulong Out = 0;

                Out |= (ulong)((N == 1) ? (1 << 3) : 0);
                Out |= (ulong)((Z == 1) ? (1 << 2) : 0);
                Out |= (ulong)((C == 1) ? (1 << 1) : 0);
                Out |= (ulong)((V == 1) ? (1 << 0) : 0);

                return Out << 28;
            }
        }

        public ulong GetX(int i)
        {
            switch (i)
            {
                case 0: return X0;
                case 1: return X1;
                case 2: return X2;
                case 3: return X3;
                case 4: return X4;
                case 5: return X5;
                case 6: return X6;
                case 7: return X7;
                case 8: return X8;
                case 9: return X9;
                case 10: return X10;
                case 11: return X11;
                case 12: return X12;
                case 13: return X13;
                case 14: return X14;
                case 15: return X15;
                case 16: return X16;
                case 17: return X17;
                case 18: return X18;
                case 19: return X19;
                case 20: return X20;
                case 21: return X21;
                case 22: return X22;
                case 23: return X23;
                case 24: return X24;
                case 25: return X25;
                case 26: return X26;
                case 27: return X27;
                case 28: return X28;
                case 29: return X29;
                case 30: return X30;
                case 31: return X31;
            }

            throw new NotImplementedException();
        }

        public void SetX(int i, ulong value)
        {
            switch (i)
            {
                case 0: X0 = value; return;
                case 1: X1 = value; return;
                case 2: X2 = value; return;
                case 3: X3 = value; return;
                case 4: X4 = value; return;
                case 5: X5 = value; return;
                case 6: X6 = value; return;
                case 7: X7 = value; return;
                case 8: X8 = value; return;
                case 9: X9 = value; return;
                case 10: X10 = value; return;
                case 11: X11 = value; return;
                case 12: X12 = value; return;
                case 13: X13 = value; return;
                case 14: X14 = value; return;
                case 15: X15 = value; return;
                case 16: X16 = value; return;
                case 17: X17 = value; return;
                case 18: X18 = value; return;
                case 19: X19 = value; return;
                case 20: X20 = value; return;
                case 21: X21 = value; return;
                case 22: X22 = value; return;
                case 23: X23 = value; return;
                case 24: X24 = value; return;
                case 25: X25 = value; return;
                case 26: X26 = value; return;
                case 27: X27 = value; return;
                case 28: X28 = value; return;
                case 29: X29 = value; return;
                case 30: X30 = value; return;
                case 31: X31 = value; return;
            }

            throw new NotImplementedException();
        }

        public Vector128<float> GetQ(int index)
        {
            switch (index)
            {
                case 0: return Q0;
                case 1: return Q1;
                case 2: return Q2;
                case 3: return Q3;
                case 4: return Q4;
                case 5: return Q5;
                case 6: return Q6;
                case 7: return Q7;
                case 8: return Q8;
                case 9: return Q9;
                case 10: return Q10;
                case 11: return Q11;
                case 12: return Q12;
                case 13: return Q13;
                case 14: return Q14;
                case 15: return Q15;
                case 16: return Q16;
                case 17: return Q17;
                case 18: return Q18;
                case 19: return Q19;
                case 20: return Q20;
                case 21: return Q21;
                case 22: return Q22;
                case 23: return Q23;
                case 24: return Q24;
                case 25: return Q25;
                case 26: return Q26;
                case 27: return Q27;
                case 28: return Q28;
                case 29: return Q29;
                case 30: return Q30;
                case 31: return Q31;
            }

            throw new NotImplementedException();
        }

        public void SetQ(int i, Vector128<float> value)
        {
            switch (i)
            {
                case 0: Q0 = value; return;
                case 1: Q1 = value; return;
                case 2: Q2 = value; return;
                case 3: Q3 = value; return;
                case 4: Q4 = value; return;
                case 5: Q5 = value; return;
                case 6: Q6 = value; return;
                case 7: Q7 = value; return;
                case 8: Q8 = value; return;
                case 9: Q9 = value; return;
                case 10: Q10 = value; return;
                case 11: Q11 = value; return;
                case 12: Q12 = value; return;
                case 13: Q13 = value; return;
                case 14: Q14 = value; return;
                case 15: Q15 = value; return;
                case 16: Q16 = value; return;
                case 17: Q17 = value; return;
                case 18: Q18 = value; return;
                case 19: Q19 = value; return;
                case 20: Q20 = value; return;
                case 21: Q21 = value; return;
                case 22: Q22 = value; return;
                case 23: Q23 = value; return;
                case 24: Q24 = value; return;
                case 25: Q25 = value; return;
                case 26: Q26 = value; return;
                case 27: Q27 = value; return;
                case 28: Q28 = value; return;
                case 29: Q29 = value; return;
                case 30: Q30 = value; return;
                case 31: Q31 = value; return;
            }

            throw new NotImplementedException();
        }

        public void SetQ(int i, void* dat)
        {
            Vector128<float> v = *(Vector128<float>*)dat;

            SetQ(i,v);
        }

        static int OffsetOF(string Name) => (int)Marshal.OffsetOf<ExecutionContext>(Name);

        public static int RegIndex(string Name) => OffsetOF(Name) >> 3;
        public static int LocalReg => RegIndex(nameof(Locals));

        public static int VectorOffset => OffsetOF(nameof(Q0));

        public static int VecIndex(string Name) => (OffsetOF(Name) - VectorOffset) >> 4;
        public static int VectorLocalIndex => VecIndex(nameof(VectorLocals)); 
    }
}
