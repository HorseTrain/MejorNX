using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.HLE.Horizon.Loaders
{
    public interface IExecutable
    {        
        public CodeSource Text      { get; set; }
        public CodeSource RoData    { get; set; }
        public CodeSource Data      { get; set; }

        public uint Mod0Offset      { get; set; }
        public uint BssSize         { get; set; }
    }
}
