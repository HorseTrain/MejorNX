using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Cpu.Utilities.Tools
{
    public class ObjectIndexer<T>
    {
        public delegate void Set(ulong index, T data);
        public delegate T Get(ulong index);

        Get get;
        Set set;
        public object parent; //This does not spark joy.

        public ObjectIndexer(Get get, Set set,object parent)
        {
            this.get = get;
            this.set = set;
            this.parent = parent;
        }

        public T this[ulong index]
        {
            get => get(index);

            set => set(index, value);
        }
    }
}
