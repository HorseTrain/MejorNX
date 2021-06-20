using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MejorNX.Cpu.Memory
{
    public unsafe class Stream
    {
        public void* BaseLocation       { get; private set; }

        public void* CurrentLocation    { get; set; }
        public ulong Location =>        ((ulong)CurrentLocation - (ulong)BaseLocation);

        protected Stream(void* BaseLocation)
        {
            this.BaseLocation = BaseLocation;
            CurrentLocation = BaseLocation;
        }

        public void Advance(ulong Size) => CurrentLocation = (void*)((ulong)CurrentLocation + Size);

        public void Seek(ulong Offset,bool Rel = false) => CurrentLocation = (void*)((ulong)BaseLocation + Offset);
    }
}
