using GalacticARM.Runtime;
using Gee.External.Capstone;
using Gee.External.Capstone.Arm64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Decoding
{
    public class AOpCode
    {
        public ulong Address                                        { get; set; }
        public int RawOpCode                                        { get; set; }
        public Emit emit                                            { get; set; }
        public Dictionary<string, InstructionInfo> InstructionData  { get; set; }

        public override string ToString()
        {
            StringBuilder Out = new StringBuilder();

            using (CapstoneArm64Disassembler disassembler = CapstoneDisassembler.CreateArm64Disassembler(Arm64DisassembleMode.LittleEndian))
            {
                byte[] binaryCode = VirtualMemoryManager.ReadObjects<byte>(Address,4);

                Arm64Instruction[] instructions = disassembler.Disassemble(binaryCode);
                foreach (Arm64Instruction instruction in instructions)
                {
                    var address = instruction.Address;
                    Arm64InstructionId id = instruction.Id;
                    if (!instruction.IsDietModeEnabled)
                    {
                        // ...
                        //
                        // An instruction's mnemonic and operand text are only available when Diet Mode is disabled.
                        // An exception is thrown otherwise!
                        var mnemonic = instruction.Mnemonic;
                        var operand = instruction.Operand;
                        Out.Append($"{mnemonic} {operand}");
                    }
                }
            }

            return Out.ToString();
        }
    }
}
