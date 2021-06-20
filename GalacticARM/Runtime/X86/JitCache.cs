using System.Runtime.InteropServices;
using System;

namespace GalacticARM.Runtime.X86
{
    public static unsafe class JitCache
    {
        [DllImport("kernel32.dll")]
        private static extern void* VirtualAlloc(void* addr, int size, int type, int protect);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(void* addr, int size, int new_protect, int* old_protect);
        [DllImport("kernel32.dll")]
        private static extern bool VirtualFree(void* addr, int size, int type);

        static byte* Base;
        const int Size = 800 * 1024 * 1024;

        public static object Lock = new object();

        static JitCache()
        {
            Base = (byte*)VirtualAlloc(null, Size, 0x1000, 4);

            int dummy;
            VirtualProtect(Base, Size, 0x40, &dummy);
        }

        static ulong _base;

        public static void GetNativeFunction(GuestFunction function)
        {
            function.Func = Marshal.GetDelegateForFunctionPointer<_func>((IntPtr)(Base + _base));

            for (ulong i = 0; i < (ulong)function.Buffer.Length; i++)
            {
                Base[i + _base] = function.Buffer[i];
            }

            _base += (ulong)function.Buffer.Length;

            if (_base >= Size)
            {
                throw new OutOfMemoryException();
            }
        }
    }
}
