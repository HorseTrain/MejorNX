using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public class ABasicBlock
    {
        public ulong Address                { get; set; }
        public List<AOpCode> Instructions   { get; set; }

        public ABasicBlock(ulong Address)
        {
            this.Address = Address;

            ulong Base = Address;

            Instructions = new List<AOpCode>();

            while (true)
            {
                (OpCodeTable Out, int RawOpCode) = OpCodeTable.GetTable(Address);

                AOpCode opCode = new AOpCode()
                {
                    Address = Address,
                    RawOpCode = RawOpCode,
                    emit = Out.emit
                };

                opCode.InstructionData = new Dictionary<string, InstructionInfo>();

                foreach (InstructionInfo info in Out.Info)
                {
                    opCode.InstructionData.Add(info.Name,info);
                }

                Instructions.Add(opCode);

                if (OpCodeTable.IsBranch(RawOpCode))
                {
                    break;
                }

                Address += 4;
            }
        }
    }
}
