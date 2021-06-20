using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.Common.Utilities
{
    //Maybe use uints instead of ulongs ?
    public class ObjectCollection
    {
        public Dictionary<ulong, object> Objects    { get; set; }

        public ObjectCollection()
        {
            Objects = new Dictionary<ulong, object>();
        }

        uint CurrentID { get; set; } = 1;

        public uint GetID()
        {
            CurrentID++;

            return CurrentID - 1;
        }

        public uint AddObject(object obj)
        {
            lock (Objects)
            {
                uint ID = GetID();

                Objects.Add(ID, obj);

                return ID;
            }
        }

        public void RemoveObject(ulong ID)
        {
            lock (Objects)
            {
                Objects.Remove(ID);
            }
        }

        public object GetObject(uint ID) => Objects[ID];

        public void SwapObject(uint Handle, object obj)
        {
            lock (Objects)
            {
                Objects[Handle] = obj;
            }
        }

        public object this[uint index] => GetObject(index);

        public void DeleteObject(uint index)
        {
            lock (Objects)
            {
                Objects.Remove(index);
            }
        }

        public bool ContainsObject(uint Index) => Objects.ContainsKey(Index);

        public void SetObject(uint Index, object Object) => Objects.Add(Index,Object);
    }
}
