using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.IntermediateRepresentation
{
    public class Operand
    {
        public OperandType Type     { get; set; }
        public ulong Data           { get; set; }

        public static Operand Reg(int reg) => new Operand() { Data = (ulong)reg, Type = OperandType.Register};
        public static Operand Const(ulong Imm) => new Operand() { Data = Imm, Type = OperandType.Immediate };
        public static Operand Vec(int reg) => new Operand() { Data = (ulong)reg, Type = OperandType.VectorRegister };

        public static implicit operator Operand(ulong Imm) => Const(Imm);
        public static implicit operator Operand(long Imm) => Const((ulong)Imm);

        public static implicit operator Operand(uint Imm) => Const(Imm);
        public static implicit operator Operand(int Imm) => Const((uint)Imm);

        public IntSize GetConstSize()
        {
            if ((Data & 255) == Data)
            {
                return IntSize.Int8;
            }

            if ((Data & ushort.MaxValue) == Data)
            {
                return IntSize.Int16;
            }

            if ((Data & uint.MaxValue) == Data)
            {
                return IntSize.Int32;
            }

            return IntSize.Int64;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case OperandType.Register: return $"R{Data}";
                case OperandType.Immediate: return $"{Data}";
                case OperandType.VectorRegister: return $"V{Data}";
                default: throw new NotImplementedException();
            }
        }
    }
}
