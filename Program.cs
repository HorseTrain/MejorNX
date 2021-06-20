using MejorNX.Common.Debugging;
using MejorNX.Cpu;
using MejorNX.HLE;
using MejorNX.HLE.Horizon;
using MejorNX.Cpu.Memory;
using static MejorNX.Cpu.Memory.VirtualMemoryManager;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Text;
using MejorNX.Window;
using Keystone;

namespace MejorNX
{
    class Program
    {
        private delegate int HelloDelegate(string msg, int ret);

        static void RunProgram(string[] args)
        {
            for (int i = 0; i < 32; i++)
            {
                //Console.WriteLine($"typeof(ThreadContext).GetField(\"X{i}\"),");
            }

            //string[] args = new string[] { @"C:\Users\Raymond\Desktop\application\application.nro" };
            //args = new string[] { @"D:\Games\Roms\Super Mario Odyssey" };
            //args = new string[] { @"D:\Games\Switch\SU" };
            args = new string[] { @"D:\Games\Switch\SM" };
            //args = new string[] { @"C:\Users\Raymond\Desktop\application\oxidgb.10-print.nso" };

            string path = args[0];

            Switch ns = new Switch();

            CpuContext.InDebugMode = false;
            CpuContext.DebugStart = 0;
            CpuContext.DebugEnd = 999999;

            //3563126

            //CpuContext.GenerateCPU = UnicornCpuContext.CreateContext;
            //CpuContext.GenerateCPU = ArmCCpuThread.CreateContext;
            CpuContext.GenerateCPU = GalacticARMCpuContext.CreateContext;

            Process process = ns.Hos.OpenProcess();

            if (path.Contains("."))
            {
                process.LoadHomebrew(path);

                Debug.Log("Loading As Homebrew.");
            }
            else
            {
                process.LoadCart(path);

                Debug.Log("Loading As Cart.");
            }

            //Console.WriteLine(GalacticARM.Arm.Memory.VirtualMemoryManager.ReadObject<int>(137826304 + 0x90));

            //new GameScreen();

            process.StartProgram();
        }

        public static void TestProgram()
        {
            Engine engine = new Engine(Architecture.ARM64,Mode.LITTLE_ENDIAN);

            byte[] Program = engine.Assemble(@"

start:
cmp x0, #20
b start

", 0).Buffer;
        }

        static unsafe void Main(string[] args)
        {
            RunProgram(null);

            //TestProgram();
        }
    }
}
