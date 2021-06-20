using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static unsafe class FallbackMemory
    {
        /*
        public static ulong GetPhysicalAddress(ulong VirtualAddress) => (ulong)VirtualMemoryManager.ReqeustPhysicalAddress(VirtualAddress);

        public static void SetExclusive_fb(ulong context, ulong address) => ExclusiveMonitors.SetExclusive((ExecutionContext*)context, address);

        public static ulong TestExclusive_fb(ulong context, ulong address) => ExclusiveMonitors.TestExclusive((ExecutionContext*)context, address);

        public static void Clrex_fb(ulong context) => ExclusiveMonitors.Clrex((ExecutionContext*)context);
        */

        static object Lock = new object();
        public const ulong ErgMask = (4 << 4) - 1;

        public static void SetExclusive_fb(ulong _context, ulong address)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            lock (Lock)
            {
                address &= ~ErgMask;

                context->ExclusiveAddress = address;
                context->ExclusiveValue = *(ulong*)address;
            }
        }

        public static ulong TestExclusive_fb(ulong _context, ulong address)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            lock (Lock)
            {
                address &= ~ErgMask;

                if (context->ExclusiveAddress == ulong.MaxValue)
                {
                    return 0;
                }

                ulong ret = (*(ulong*)address == context->ExclusiveValue) ? 1UL : 0;

                return ret;
            }
        }

        public static void Clrex_fb(ulong _context)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            context->ExclusiveAddress = ulong.MaxValue;
        }


    }
}
