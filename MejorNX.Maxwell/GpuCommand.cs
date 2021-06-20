using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Maxwell
{
    public struct GpuCommand
    {
        public int Method       { get; set; }
        public int SubChannel   { get; set; }
        public int[] Arguments  { get; set; }

        public GpuCommand(int Method, int SubChannel, params int[] Arguments)
        {
            this.Method = Method;
            this.SubChannel = SubChannel;
            this.Arguments = Arguments;
        }
    }
}
