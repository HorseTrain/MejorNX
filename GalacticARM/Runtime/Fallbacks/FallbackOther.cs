using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static class FallbackOther
    {
        static Stopwatch _tickCounter;

        static FallbackOther()
        {
            _tickCounter = new Stopwatch();
            
            _tickCounter.Start();
        }

        public static ulong GetCntpctEl0()
        {
            double ticks = _tickCounter.ElapsedTicks * (1.0 / Stopwatch.Frequency);

            return (ulong)(ticks * 19200000);
        }
    }
}
