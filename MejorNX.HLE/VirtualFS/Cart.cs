using System;
using System.IO;

namespace MejorNX.HLE.VirtualFS
{
    public class Cart : IDisposable
    {
        public FileStream stream    { get; set; }

        public Cart(string romfs)
        {
            stream = new FileStream(romfs,FileMode.Open,FileAccess.Read);
        }

        public void Dispose()
        {
            stream.Close();
        }
    }
}
