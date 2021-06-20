using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.IntermediateRepresentation
{
    public class OperationBlock
    {
        public List<Operation> Operations   { get; set; }

        public OperationBlock()
        {
            Operations = new List<Operation>();

            marked = new List<Operand>();
        }

        public Operation[] ExtractOperations => Operations.ToArray();

        public Operation AddInstruction(Operation operation)
        {
            operation.Address = Operations.Count;

            Operations.Add(operation);

            return operation;
        }

        public Operation AddInstruction(Instruction instruction, params Operand[] operands) => AddInstruction(new Operation(instruction,operands));
        public Operation AddInstruction(Instruction instruction,IntSize Size, params Operand[] operands) => AddInstruction(new Operation(instruction, operands) { Size = Size});

        public override string ToString()
        {
            StringBuilder Out = new StringBuilder();

            //Out.AppendLine($"Address: {BaseAddress}");

            foreach (Operation operation in Operations)
            {
                Out.AppendLine(operation.ToString());
            }

            return Out.ToString();
        }

        List<Operand> marked;

        public Operand CreateLabel()
        {
            Operand Out = new Operand();

            Out.Type = OperandType.Immediate;

            return Out;
        }

        public void MarkLabel(Operand Label)
        {
            if (marked.Contains(Label))
            {
                throw new Exception();
            }

            Label.Type = OperandType.Immediate;
            Label.Data = (ulong)Operations.Count;

            marked.Add(Label);
        }

        void AssertBool(bool src)
        {
            if (!src)
            {
                throw new Exception();
            }
        }

        public void AssertIsRegister(Operand Source) => AssertBool(Source.Type == OperandType.Register);
        public void AssertIsImm(Operand Source) => AssertBool(Source.Type == OperandType.Immediate);

    }
}
