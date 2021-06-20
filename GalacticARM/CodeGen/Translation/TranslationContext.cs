using GalacticARM.CodeGen.X86;
using GalacticARM.Decoding;
using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using GalacticARM.Runtime.Fallbacks;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GalacticARM.CodeGen.Translation
{
    public class TranslationContext : OperationBlock 
    {
        public IntSize CurrentSize              { get; set; }
        public int BitCount => CurrentSize == IntSize.Int32 ? 32 : 64;
        public AOpCode CurrentOpCode            { get; set; }
        bool KnownReturn                        { get; set; }

        int LocalIndex;
        int VectorLocalIndex;

        public List<Operand> KnwonReturns;
        public Dictionary<ulong, Operand> Blocks;

        public Operand ReturnLocal;

        public ABasicBlock CurrentBlock;

        public TranslationContext()
        {
            CurrentSize = IntSize.Int64;

            Advance();

            Blocks = new Dictionary<ulong, Operand>();
        }

        public void Advance()
        {
            LocalIndex = ExecutionContext.LocalReg;
            VectorLocalIndex = ExecutionContext.VectorLocalIndex;
        }

        public void SetReturn(Operand operand)
        {
            if (operand.Type == OperandType.Immediate)
            {
                KnwonReturns.Add(operand);
            }

            SetRegRaw(nameof(ExecutionContext.Return),operand);
        }

        public void AdvancePC()
        {
            SetReturn(CurrentOpCode.Address + 4);
        }

        public void Return(Operand operand)
        {
            AddInstruction(Instruction.Return,operand);
        }

        public void EnsureRegister(Operand test) => EnsureBool(test.Type == OperandType.Register);

        public void EnsureIsVector(Operand test) => EnsureBool(test.Type == OperandType.VectorRegister);

        public void EnsureBool (bool test)
        {
            if (!test)
            {
                throw new Exception();
            }
        }

        public Operand MoveWithOperation(Instruction instruction, Operand operand0, Operand operand1)
        {
            Operand des = AllocateLocal();

            EnsureRegister(operand0);

            AddInstruction(new Operation(Instruction.Move,des,operand0));

            Operation o = new Operation(instruction, des, operand1);

            o.Size = CurrentSize;

            AddInstruction(o);

            return des;
        }

        public Operand MoveWithOperation(Instruction instruction,Operand operand0)
        {
            Operand des = AllocateLocal();

            EnsureRegister(operand0);

            AddInstruction(new Operation(Instruction.Move, des, operand0));

            Operation o = new Operation(instruction, des);

            o.Size = CurrentSize;

            AddInstruction(o);

            return des;
        }

        public (int, InstructionInfo) GetRawInt(string Name)
        {
            InstructionInfo info = CurrentOpCode.InstructionData[Name];

            int reg = ((CurrentOpCode.RawOpCode >> info.Index) & info.Mask);

            return (reg, info);
        }

        public void SetSize(string Name)
        {
            if (CurrentOpCode.InstructionData.ContainsKey(Name))
            {
                (int dat, InstructionInfo info) = GetRawInt(Name);

                CurrentSize = IntSize.Int64;

                if (dat == 0)
                {
                    CurrentSize = IntSize.Int32;
                }

                return;
            }

            throw new Exception();
        }

        public int GetRaw(string Name)
        {
            if (CurrentOpCode.InstructionData.ContainsKey(Name))
            {
                (int reg, InstructionInfo info) = GetRawInt(Name);

                return reg;
            }

            throw new Exception(Name);
        }

        public Operand GetRegister(int reg, bool AccountForSp = false)
        {
            if (reg > 31)
            {
                throw new Exception();
            }

            if (!((reg == 31) && !AccountForSp))
            {
                return GetRegRaw($"X{reg}");
            }
            else
            {
                return Const(0);
            }
        }

        public void SetRegister(int reg, Operand d, bool AccountForSp = false)
        {
            if (reg > 31)
            {
                throw new Exception();
            }

            if (!((reg == 31) && !AccountForSp))
            {
                SetRegRaw($"X{reg}",d);
            }
        }

        public Operand GetRegister(string Name)
        {
            if (CurrentOpCode.InstructionData.ContainsKey(Name))
            {
                (int reg, InstructionInfo info) = GetRawInt(Name);

                return GetRegister(reg, info.IsSP);
            }

            throw new Exception(Name);
        }

        public void SetRegister(string Name, Operand d)
        {
            if (CurrentOpCode.InstructionData.ContainsKey(Name))
            {
                (int reg, InstructionInfo info) = GetRawInt(Name);
                
                SetRegister(reg,d,info.IsSP);

                return;
            }

            throw new Exception(Name);
        }

        public Operand GetRegRaw(string Name)
        {
            int reg = ExecutionContext.RegIndex(Name);

            Operand Local = AllocateLocal();

            AddInstruction(Instruction.Move,CurrentSize, Local, Operand.Reg(reg));

            return Local;
        }

        public void SetRegRaw(string Name, Operand d)
        {
            int reg = ExecutionContext.RegIndex(Name);

            AddInstruction(Instruction.Move,CurrentSize,Operand.Reg(reg),d);
        }

        public Operand ThrowUnknown()
        {
            Console.WriteLine(CurrentOpCode);

            throw new NotImplementedException();
        }

        Operand GetReg(int Local) => new Operand() { Type = OperandType.Register, Data = (ulong)Local };

        public unsafe int AllocateLocalIndex()
        {
            LocalIndex++;

            if (LocalIndex >= sizeof(LocalStore) >> 3)
            {
                throw new Exception();
            }

            return LocalIndex;
        }

        public unsafe int AllocateVectorLocalIndex()
        {
            VectorLocalIndex++;

            if (VectorLocalIndex >= sizeof(LocalStore) >> 4)
            {
                throw new Exception();
            }

            return VectorLocalIndex;
        }

        public Operand AllocateLocal() => GetReg(AllocateLocalIndex());

        public GuestFunction CompileFunction() => new GAssembler(this).Compile();

        public Operand Add(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Add, Arg0, Arg1);
        public Operand And(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.And, Arg0, Arg1);
        public Operand Ceq(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Ceq, Arg0, Arg1);
        public Operand Cgt(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Cgt, Arg0, Arg1);
        public Operand Cgt_Un(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Cgt_Un, Arg0, Arg1);
        public Operand Cgte(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Cgte, Arg0, Arg1);
        public Operand Cgte_Un(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Cgte_Un, Arg0, Arg1);
        public Operand Clt(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Clt, Arg0, Arg1);
        public Operand Clt_Un(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Clt_Un, Arg0, Arg1);
        public Operand Clte(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Clte, Arg0, Arg1);
        public Operand Clte_Un(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Clte_Un, Arg0, Arg1);
        public Operand Divide(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Divide, Arg0, Arg1);
        public Operand Divide_Un(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Divide_Un, Arg0, Arg1);
        public Operand Load16(Operand Arg0) => MoveWithOperation(Instruction.Load16, Arg0);
        public Operand Load32(Operand Arg0) => MoveWithOperation(Instruction.Load32, Arg0);
        public Operand Load64(Operand Arg0) => MoveWithOperation(Instruction.Load64, Arg0);
        public Operand Load8(Operand Arg0) => MoveWithOperation(Instruction.Load8, Arg0);
        public Operand LoadContext(Operand Arg0) => MoveWithOperation(Instruction.LoadContext, Arg0);
        public Operand Move(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Move, Arg0, Arg1);
        public Operand Multiply(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Multiply, Arg0, Arg1);
        public Operand Not(Operand Arg0) => MoveWithOperation(Instruction.Not, Arg0);
        public Operand Or(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Or, Arg0, Arg1);
        public Operand Return(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Return, Arg0, Arg1);
        public Operand ShiftLeft(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.ShiftLeft, Arg0, Arg1);
        public Operand ShiftRight(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.ShiftRight, Arg0, Arg1);
        public Operand ShiftRight_Singed(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.ShiftRight_Singed, Arg0, Arg1);
        public Operand SignExtend16(Operand Arg0) => MoveWithOperation(Instruction.SignExtend16, Arg0);
        public Operand SignExtend32(Operand Arg0) => MoveWithOperation(Instruction.SignExtend32, Arg0);
        public Operand SignExtend8(Operand Arg0) => MoveWithOperation(Instruction.SignExtend8, Arg0);

        public void Store16(Operand Arg0, Operand Arg1) => AddInstruction(Instruction.Store16, Arg0, Arg1);
        public void Store32(Operand Arg0, Operand Arg1) => AddInstruction(Instruction.Store32, Arg0, Arg1);
        public void Store64(Operand Arg0, Operand Arg1) => AddInstruction(Instruction.Store64, Arg0, Arg1);
        public void Store8(Operand Arg0, Operand Arg1) => AddInstruction(Instruction.Store8, Arg0, Arg1);

        public Operand Subtract(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Subtract, Arg0, Arg1);
        public Operand Xor(Operand Arg0, Operand Arg1) => MoveWithOperation(Instruction.Xor, Arg0, Arg1);
        public Operand Clz(Operand d) => Clt(d,0); //And(ShiftRight(d, BitCount - 1), 1);
        public Operand Const(ulong c) => CreateLocal(c);
        public void Jump(Operand Label)
        {
            AssertIsImm(Label);

            AddInstruction(Instruction.Jump, Label);
        }
        public void JumpIf(Operand Test, Operand Label)
        {
            AssertIsRegister(Test);
            AssertIsImm(Label);

            AddInstruction(Instruction.JumpIf, Test, Label);            
        }
        public Operand CreateLocal(Operand o)
        {
            Operand Out = AllocateLocal();

            AddInstruction(Instruction.Move,Out,o);

            return Out;
        }

        public void Nop() => AddInstruction(Instruction.Nop);

        public Operand InvertBool(Operand test) => Xor(test,1);

        public Operand GetFunctionPointer(string Name)
        {
            //return Const(DelegateCache.GetFunctionPointer(Name));

            bool reset = false;

            if (CurrentSize == IntSize.Int32)
            {
                reset = true;

                CurrentSize = IntSize.Int64;
            }

            Operand Pointer = GetRegRaw(nameof(ExecutionContext.FunctionTablePointer));

            AddInstruction(Instruction.Add, Pointer,DelegateCache.GetFunctionIndex(Name) << 3);

            AddInstruction(Instruction.Load64,Pointer,Pointer);

            if (reset)
            {
                CurrentSize = IntSize.Int32;
            }

            return Pointer;
        }

        public Operand Call(string Name,params Operand[] operands)
        {
            Operand FunctionPointer = GetFunctionPointer(Name);

            Operand[] args = new Operand[operands.Length + 1];

            args[0] = FunctionPointer;

            for (int i = 0; i < operands.Length; i++)
            {
                if (operands[i].Type == OperandType.Immediate)
                {
                    operands[i] = Const(operands[i].Data);
                }

                args[i + 1] = operands[i];
            }

            CallRaw(args);

            return FunctionPointer;
        }

        public void CallRaw(params Operand[] args)
        {
            AddInstruction(Instruction.Call, args);
        }

        public void SetArgument(int arg, Operand data)
        {
            SetRegRaw($"Arg{arg}",data);
        }

        public Operand ContextPointer() => GetRegRaw(nameof(ExecutionContext.MyPointer));

        //Vector
        public Operand CreateVector()
        {
            Operand Out = Operand.Vec(AllocateVectorLocalIndex());

            ClearVector(Out);

            return Out;
        }

        public void ClearVector(Operand Vector)
        {
            EnsureIsVector(Vector);

            AddInstruction(Instruction.Vector_ClearVector, Vector);
        }

        public void SetVectorElement(Operand Vector, Operand Source, int Index, int Size, bool Clear = false)
        {
            EnsureIsVector(Vector);
            EnsureBool(Size < 4);

            EnsureBool(Index < (1 << (4 - Size)));

            if (Clear)
            {
                ClearVector(Vector);
            }

            Operation o = AddInstruction(Instruction.Vector_SetVectorElement, Vector, Source, Index, Size);

            if (Size < 3)
            {
                o.Size = IntSize.Int32;
            }
            
            if (Size == 3)
            {
                o.Size = IntSize.Int64;
            }
        }

        public void ConvertToFloat(Operand Des,Operand Src, int from, int to, bool singed)
        {
            if (singed)
            {
                Operation o = AddInstruction(Instruction.Vector_ConvertToFloat, Des, Src, from, to, singed ? 1 : 0);

                if (from == 2)
                {
                    o.Size = IntSize.Int32;
                }
                else
                {
                    o.Size = IntSize.Int64;
                }
            }
            else
            {
                SetArgument(0, Des.Data);
                SetArgument(1, Src.Data);
                SetArgument(2, from);
                SetArgument(3, to);

                Call(nameof(FallbackFloat.UnsingedToFloat),ContextPointer());
            }
        }

        public Operand ConverToInt(Operand Src, int from, int to)
        {
            Operand des = AllocateLocal();

            Operation o = AddInstruction(Instruction.Vector_ConvertToInt,des,Src,from,to);

            if (to == 2)
            {
                o.Size = IntSize.Int32;
            }
            else
            {
                o.Size = IntSize.Int64;
            }

            return des;
        }

        public Operand GetVectorElement(Operand Vector, int Index, int Size, bool singed = false)
        {
            EnsureIsVector(Vector);

            EnsureBool(Index < (1 << (4 - Size)));
            EnsureBool(Size < 4);

            Operand Out = AllocateLocal();

            Operation o = AddInstruction(Instruction.Vector_Extract,Out,Vector,Index,Size);

            if (Size < 3)
            {
                o.Size = IntSize.Int32;
            }

            if (Size == 3)
            {
                o.Size = IntSize.Int64;
            }

            if (singed)
            {
                switch (Size)
                {
                    case 0: Out = SignExtend8(Out); break;
                    case 1: Out = SignExtend16(Out); break;
                    case 2: Out = SignExtend32(Out); break;
                }
            }

            return Out;
        }

        public Operand VectorOperation(Operand n, Operand m, Instruction instruction, bool ClearTop = false)
        {
            Operand des = CreateVector();

            AddInstruction(Instruction.Vector_Move,des,n);

            AddInstruction(instruction,des,m);

            if (ClearTop)
            {
                SetVectorElement(des,0,1,3);
            }

            return des;
        }

        public Operand ScalarOperation(Operand n, Operand m,int size, Instruction instruction)
        {
            Operand des = CreateVector();

            AddInstruction(Instruction.Vector_Move, des, n);

            AddInstruction(Instruction.Vector_ScalarOperation,(int)instruction,des,m,size);

            return des;
        }

        public Operand FloatVectorOperation(Operand n, Operand m, int size, Instruction instruction)
        {
            Operand des = CreateVector();

            AddInstruction(Instruction.Vector_Move, des, n);

            AddInstruction(Instruction.Vector_FloatVectorOperation,(int)instruction,des,m,size);

            return des;
        }

        public Operand FloatVectorOperation(Operand n, int size, Instruction instruction)
        {
            Operand des = CreateVector();

            AddInstruction(Instruction.Vector_Move, des, n);

            AddInstruction(Instruction.Vector_FloatVectorOperation, (int)instruction,des,null, size);

            return des;
        }

        public void VectorNot(Operand d)
        {
            for (int i = 0; i < 2; i++)
            {
                Operand n = GetVectorElement(d,i,3);

                n = Not(n);

                SetVectorElement(d,n,i,3);
            }
        }

        public void SetVector(int des, Operand Vector)
        {
            EnsureIsVector(Vector);

            AddInstruction(Instruction.Vector_Move, Operand.Vec(des), Vector);
        }

        public Operand GetVector(int source)
        {
            Operand Out = CreateVector();

            AddInstruction(Instruction.Vector_Move, Out, Operand.Vec(source));

            return Out;
        }

        public Operand CreateVectorWith(Operand Source, int size)
        {
            Operand Out = CreateVector();

            SetVectorElement(Out,Source,0,size);

            return Out;
        }

        public Operand LoadVector(Operand Address, int size)
        {
            Operand Out = CreateVector();

            AddInstruction(Instruction.Vector_Load,Out,Address,size);

            return Out;
        }

        public Operand FillVectorWith(Operand Source, int size)
        {
            Operand Out = CreateVector();

            int count = 16 >> size;

            for (int i = 0; i < count; i++)
            {
                SetVectorElement(Out,Source,i,size);
            }

            return Out;
        }

        public void StoreVector(Operand Vector, Operand Address, int size)
        {
            EnsureIsVector(Vector);

            AddInstruction(Instruction.Vector_Store,Address,Vector,size);
        }

        public void ReturnNil()
        {
            SetRegRaw(nameof(ExecutionContext.IsExecuting), 0);

            SetArgument(0, ulong.MaxValue);

            Return(CurrentOpCode.Address);
        }
    }
}
