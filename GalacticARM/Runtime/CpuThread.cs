using GalacticARM.CodeGen.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnicornNET;

namespace GalacticARM.Runtime
{
    public unsafe delegate void SVC(int id);

    public unsafe class CpuThread
    {
        internal UnicornCpuThread ucf;

        public ref ExecutionContext Context => ref *((ExecutionContext*)NativeContext);

        IntPtr NativeContext;

        static CpuThread()
        {
            Counter = new Stopwatch();

            Counter.Start();
        }

        internal CpuThread(int Handle)
        {
            InitContext(Handle);

            InitUnicorn();
        }

        void InitContext(int Handle)
        {
            NativeContext = Marshal.AllocHGlobal(sizeof(ExecutionContext));

            Context = new ExecutionContext();

            Context.ID = (ulong)Handle;

            Context.FunctionTablePointer = DelegateCache.FunctionTablePointer;

            Console.WriteLine($"Created Thread {Handle}");
        }

        void InitUnicorn()
        {
            ucf = new UnicornCpuThread(this);
        }

        static HashSet<int> Handles = new HashSet<int>();
        public static Dictionary<int, CpuThread> Threads = new Dictionary<int, CpuThread>();

        public static CpuThread CreateThread()
        {
            int handle = 0;

            lock (Handles)
            {
                while (true)
                {
                    if (!Handles.Contains(handle))
                    {
                        Handles.Add(handle);

                        Threads.Add(handle, new CpuThread(handle));

                        return Threads[handle];
                    }

                    handle++;
                }
            }
        }

        public ulong Execute(ulong Entry, bool Once = false)
        {
            Context.MemoryPointer = (ulong)VirtualMemoryManager.PageMap;

            if (false)
            {
                ucf.Execute(Entry);

                return ucf.Engine.PC;
            }

            Context.IsExecuting = 1;

            Context.MyPointer = (ulong)NativeContext;

            while (true)
            {
                Entry = ExecuteSingle(Entry);

                if (Once || Context.IsExecuting == 0)
                {
                    return Entry;
                }
            }
        }

        public ulong ExecuteSingle(ulong Entry)
        {
            GuestFunction function = Translator.GetOrTranslateFunction(Entry);

            Context.MyPointer = (ulong)NativeContext;

            return function.Execute((ExecutionContext*)Context.MyPointer);
        }

        public SVC svc;

        public static void CallSVC(ulong ContextPointer, ulong id)
        {
            ExecutionContext* context = (ExecutionContext*)ContextPointer;

            Threads[(int)context->ID].svc((int)id);
        }

        public static Stopwatch Counter;

        public static ulong GetCntpctEl0() => (ulong)(Counter.ElapsedTicks * (1.0 / Stopwatch.Frequency)) * 19200000;

        public void EndExecution()
        {
            Context.IsExecuting = 0;

            if (ucf != null)
            {
                ucf.End();
            }
        }

        ~CpuThread()
        {
            Marshal.FreeHGlobal(NativeContext);
        }
    }
}