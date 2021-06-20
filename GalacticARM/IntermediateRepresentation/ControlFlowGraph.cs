using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.IntermediateRepresentation
{
    public class Node
    {
        public ulong BaseAddress;

        public OperationBlock BasicBlock;

        public Node Branch;
        public Node Next;

        public Node(OperationBlock block)
        {
            BasicBlock = block;
        }
    }

    public class ControlFlowGraph
    {
        public OperationBlock SourceBlock;

        Dictionary<int, Node> Blocks    { get; set; }
        public List<Node> Nodes         { get; set; }

        public ControlFlowGraph(OperationBlock source)
        {
            this.SourceBlock = source;

            Blocks = new Dictionary<int, Node>();
            Nodes = new List<Node>();

            GetBasicBlocks(0);

            while (true)
            {
                if (Que.Count == 0)
                    break;

                int[] tmp = Que.ToArray();

                foreach (int i in tmp)
                {
                    Que.Remove(i);

                    GetBasicBlocks(i);
                }
            }
        }

        List<int> Que = new List<int>();

        Node GetBasicBlocks(int Address)
        {
            if (Blocks.ContainsKey(Address))
            {
                return Blocks[Address];
            }

            if (!(Address < SourceBlock.Operations.Count))
            {
                return null;
            }

            OperationBlock block = new OperationBlock();

            Node Out = new Node(block);

            Out.BaseAddress = (ulong)Address;

            Blocks.Add(Address, new Node(block));

            for (int i = Address; i < SourceBlock.Operations.Count; i++)
            {
                Operation o = SourceBlock.Operations[i];

                block.Operations.Add(o);

                if (o.Instruction.ToString().Contains("Jump"))
                {
                    int New = 0;

                    if (o.Instruction == IntermediateRepresentation.Instruction.Jump)
                    {
                        New = (int)o.Operands[0].Data;
                    }
                    else if (o.Instruction == IntermediateRepresentation.Instruction.JumpIf)
                    {
                        New = (int)o.Operands[1].Data;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    Que.Add(i+1);
                    Que.Add(New);

                    //Out.Next = GetBasicBlocks(i + 1);
                    //Out.Branch = GetBasicBlocks(New);

                    break;
                }
            }

            Nodes.Add(Out);

            return Out;
        }

        public Node GetBlock(int Address) => Blocks[Address];

        public bool Contains(int Address) => Blocks.ContainsKey(Address);

        public override string ToString()
        {
            StringBuilder Out = new StringBuilder();

            foreach (Node node in Nodes)
            {
                Out.AppendLine(node.BasicBlock.ToString());
            }

            return Out.ToString();
        }
    }
}
