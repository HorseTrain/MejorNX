using GalacticARM.CodeGen.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static unsafe class FallbackCF
    {
        public static ulong GetSoftJump(ulong _context,ulong Address)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            if (context->IsExecuting == 0)
            {
                throw new Exception();
            }

            context->Arg0 = 0;
            return 0;

            GuestFunction func;

            if (Translator.Functions.TryGetValue(Address,out func))
            {
                context->Arg0 = 1;

                return func.Ptr;
            }

            context->Arg0 = 0;

            return 0;
        }
    }
}
