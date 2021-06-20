using GalacticARM.IntermediateRepresentation;
using GalacticARM.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.CodeGen.Translation
{
    public static class EmitUniversal
    {
        public delegate void IfTest();

        public static void EmitIf(TranslationContext context, Operand test, IfTest yes = null, IfTest no = null)
        {
            Operand end = context.CreateLabel();
            Operand succ = context.CreateLabel();

            context.JumpIf(test,succ);

            if (no != null)
                no();

            context.Jump(end);

            context.MarkLabel(succ);

            if (yes != null)
                yes();

            context.MarkLabel(end);
        }

        public static bool UseUnicorn = true;

        public static void EmitUnicornFB(TranslationContext context)
        {
            if (UseUnicorn)
            {
                //context.ReturnNil();

                //return;

                Console.WriteLine($"Instruction: {context.CurrentOpCode} Resorting to Unicorn.");

                Operand Return = context.Call(nameof(UnicornCpuThread.FallbackStepUni), context.ContextPointer(), context.CurrentOpCode.Address);

                if (UnicornCpuThread.StepCount != 1)
                    context.Return(Return);
            }    
            else
            {
                throw new NotImplementedException($"Unknown Instruction {VirtualMemoryManager.GetOpHex(context.CurrentOpCode.Address)} {Convert.ToString(VirtualMemoryManager.ReadObject<uint>(context.CurrentOpCode.Address),2)}");
            }

        }
    }
}
