using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.Cpu.Utilities.Tools
{
    public static class StringTools
    {
        public static string FillStringFront(object src, char fill,int wantedsize)
        {
            string source = src.ToString();

            while (source.Length < wantedsize)
                source = fill + source;

            return source;
        }

        public static string FillStringBack(object src, char fill, int wantedsize)
        {
            string source = src.ToString();

            while (source.Length < wantedsize)
                source += fill;

            return source;
        }
    }
}
