namespace MejorNX.Cpu.Memory
{
    public unsafe class MemoryReader : Stream
    {
        public MemoryReader(void* Location) : base (Location)
        {

        }

        public T ReadStruct<T>() where T: unmanaged
        {
            T Out = *(T*)CurrentLocation;

            Advance((ulong)sizeof(T));

            return Out;
        }

        public T[] ReadArray<T>(ulong Size) where T: unmanaged
        {
            T[] Out = new T[Size];

            for (ulong i = 0; i < Size; i++)
                Out[i] = ReadStruct<T>();

            return Out;
        }

        public T ReadStruct<T>(ulong Address) where T : unmanaged
        {
            Seek(Address);

            return ReadStruct<T>();
        }

        //TODO: Change this to "Address"
        public T ReadStructAtOffset<T>(ulong Offset) where T : unmanaged
        {
            Seek(Offset);

            return ReadStruct<T>();
        }

        public string ReadString(ulong size = ulong.MaxValue, bool unicode = false)
        {
            string Out = "";

            for (ulong i = 0; i < size; i++)
            {
                dynamic temp = 0;

                if (!unicode)
                    temp = ReadStruct<byte>();
                else
                    temp = ReadStruct<char>();

                if (temp == 0)
                    break;

                if (temp > 0x20 && temp < 0x7f)
                    Out += (char)temp;
            }    

            return Out;
        }

        

        public string ReadStringAtAddress(ulong Address, ulong size = ulong.MaxValue)
        {
            Seek(Address);

            return ReadString(size);
        }
    }
}
