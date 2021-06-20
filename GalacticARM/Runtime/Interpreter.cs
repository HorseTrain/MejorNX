using GalacticARM.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime
{
    public static class Interpreter
    {
        public static unsafe ulong IntTest(OperationBlock block, ExecutionContext* context)
        {
            int index = 0;

            while (true)
            {
                Operation cop = block.Operations[index];

                dynamic GetData(int arg, bool singed = false)
                {
                    ulong Out = 0;

                    Operand o = cop.Operands[arg];

                    if (o.Type == OperandType.Register)
                    {
                        Out = ((ulong*)context)[o.Data];
                    }
                    else if (o.Type == OperandType.Immediate)
                    {
                        Out = o.Data;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (cop.Size == IntSize.Int32)
                    {
                        if (!singed)
                            return (uint)Out;

                        return (int)(uint)Out;
                    }

                    if (singed)
                        return (long)Out;

                    return Out;
                }

                void SetData(int arg, dynamic dat)
                {
                    Operand o = cop.Operands[arg];

                    if (o.Type != OperandType.Register)
                    {
                        throw new Exception();
                    }

                    if (cop.Size == IntSize.Int32)
                    {
                        ((ulong*)context)[o.Data] = (uint)(ulong)dat;
                    }
                    else
                    {
                        ((ulong*)context)[o.Data] = (ulong)dat;
                    }
                }

                bool Advance = true;

                switch (cop.Instruction)
                {
                    case Instruction.Add: SetData(0, GetData(0) + GetData(1)); break;
                    case Instruction.And: SetData(0, GetData(0) & GetData(1)); break;

                    case Instruction.Or: SetData(0, GetData(0) | GetData(1)); break;
                    case Instruction.Subtract: SetData(0, GetData(0) - GetData(1)); break;
                    case Instruction.Xor: SetData(0, GetData(0) ^ GetData(1)); break;

                    case Instruction.SignExtend8: SetData(0, (long)(sbyte)GetData(0)); break;
                    case Instruction.SignExtend16: SetData(0, (long)(short)GetData(0)); break;
                    case Instruction.SignExtend32: SetData(0, (long)(int)GetData(0)); break;

                    case Instruction.Move: SetData(0, GetData(1)); break;
                    case Instruction.Jump:

                        Advance = false;

                        index = (int)GetData(0);

                        break;

                    case Instruction.JumpIf:

                        if ((int)GetData(0) == 1)
                        {
                            index = (int)GetData(1);

                            Advance = false;
                        }

                        break;
                    case Instruction.Ceq: SetData(0, GetData(0, true) == GetData(1, true) ? 1 : 0); break;

                    case Instruction.Clt: SetData(0, GetData(0, true) < GetData(1, true) ? 1 : 0); break;
                    case Instruction.Cgt: SetData(0, GetData(0, true) > GetData(1, true) ? 1 : 0); break;
                    case Instruction.Clte: SetData(0, GetData(0, true) <= GetData(1, true) ? 1 : 0); break;
                    case Instruction.Cgte: SetData(0, GetData(0, true) >= GetData(1, true) ? 1 : 0); break;

                    case Instruction.Clt_Un: SetData(0, GetData(0) < GetData(1) ? 1 : 0); break;
                    case Instruction.Cgt_Un: SetData(0, GetData(0) > GetData(1) ? 1 : 0); break;
                    case Instruction.Clte_Un: SetData(0, GetData(0) <= GetData(1) ? 1 : 0); break;
                    case Instruction.Cgte_Un: SetData(0, GetData(0) >= GetData(1) ? 1 : 0); break;

                    case Instruction.ShiftLeft: SetData(0, GetData(0) << (int)GetData(1)); break;
                    case Instruction.ShiftRight: SetData(0, GetData(0) >> (int)GetData(1)); break;
                    case Instruction.ShiftRight_Singed: SetData(0, GetData(0, true) >> (int)GetData(1)); break;

                    case Instruction.Not: SetData(0, ~GetData(0)); break;

                    case Instruction.Return: return GetData(0);
                    default: throw new NotImplementedException(cop.Instruction.ToString());
                }

                if (Advance)
                    index++;
            }
        }
    }
}
