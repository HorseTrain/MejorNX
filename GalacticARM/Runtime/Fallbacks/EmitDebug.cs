using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Runtime.Fallbacks
{
    public static unsafe class EmitDebug
    {
        static StreamWriter writer = new StreamWriter(@"D:\Debug\GSteps.txt");

        public static void DebugStep(ulong Context, ulong Address)
        {

        }
    }
}
