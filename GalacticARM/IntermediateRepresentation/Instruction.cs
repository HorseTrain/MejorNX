using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.IntermediateRepresentation
{
    public enum Instruction
    {
        Add,
        And,
        Call,
        Ceq,
        Cgt,
        Cgt_Un,
        Cgte,
        Cgte_Un,
        Clt,
        Clt_Un,
        Clte,
        Clte_Un,
        Divide,
        Divide_Un,
        Jump,
        JumpIf,
        Load16,
        Load32,
        Load64,
        Load8,
        LoadContext,
        Move,
        Multiply,
        Nop,
        Not,
        Or,
        Return,
        ShiftLeft,
        ShiftRight,
        ShiftRight_Singed,
        SignExtend16,
        SignExtend32,
        SignExtend8,
        Store16,
        Store32,
        Store64,
        Store8,
        Subtract,
        Xor,

        HardPC,

        Vector_SetVectorElement,
        Vector_ClearVector,
        Vector_Move,
        Vector_Load,
        Vector_Store,
        Vector_Extract,
        Vector_Or,
        Vector_And,
        Vector_Xor,
        Vector_Not,
        Vector_ConvertToFloat,
        Vector_ConvertToInt, //Singed!!

        Vector_ScalarOperation,
        Vector_FloatVectorOperation,
        Vector_FloatVectorOperationSingle,

        Vector_ScalarUnaryOperation,

        Vector_Neg,
        Vector_ConvertPerc,

        Vector_Fadd,
        Vector_Fmul,
        Vector_Fdiv,
        Vector_Fsub,
        Vector_Fsqrt,
        Vector_Frsqrt,

        Vector_Fceq,
        Vector_Fcge,
        Vector_Fcgt
    }
}
