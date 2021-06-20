using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.IntermediateRepresentation
{
    public class Operation
    {
        public int Address              { get; set; }
        public IntSize Size             { get; set; }
        public Instruction Instruction  { get; set; }
        public Operand[] Operands       { get; set; }

        public Operation(Instruction Instruction, params Operand[] Operands)
        {
            this.Instruction = Instruction;
            this.Operands = Operands;
        }

        public override string ToString()
        {
            StringBuilder Out = new StringBuilder();

            Out.Append($"{Address:d3}: {Instruction}");

            for (int i = 0; i < Operands.Length; i++)
            {
                Out.Append($" {Operands[i]}");

                if (i != Operands.Length - 1)
                {
                    Out.Append(",");
                }
            }

            return Out.ToString();
        }
    }
}
