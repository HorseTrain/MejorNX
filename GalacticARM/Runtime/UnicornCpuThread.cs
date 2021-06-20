using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using UnicornNET;

namespace GalacticARM.Runtime
{
    public unsafe class UnicornCpuThread
    {
        public Arm64Engine Engine   { get; set; }
        public CpuThread Dios       { get; set; }
        int MapCount = 0;

        public UnicornCpuThread(CpuThread Parent)
        {
            Engine = new Arm64Engine();

            Engine.AddHookSVC(CallSVC);

            SyncMemory();

            this.Dios = Parent;
        }

        public void SyncMemory()
        {
            if (MapCount != VirtualMemoryManager.Maps.Count)
            {
                lock (VirtualMemoryManager.Maps)
                {
                    for (int i = 0; i < VirtualMemoryManager.Maps.Count; i++)
                    {
                        MemoryMap map = VirtualMemoryManager.Maps[i];

                        uc.uc_mem_map_ptr(Engine.context, map.VirtualAddress, map.Size, uc_prot.UC_PROT_ALL, map.PhysicalAddress);
                    }

                    MapCount = VirtualMemoryManager.Maps.Count;
                }
            }
        }

        public void Execute(ulong Entry)
        {
            Engine.PC = Entry;

            SyncUni();

            Engine.Step(0);
        }

        public static int StepCount = 5;

        static HashSet<int> instructions = new HashSet<int>();

        public ulong StepUni(ulong PC)
        {
            SyncUni();

            SyncMemory();

            Engine.PC = PC;

            if (false)
            {
                for (int i = 0; i < StepCount; i++)
                {
                    int op = VirtualMemoryManager.ReadObject<int>(Engine.PC);

                    if (!instructions.Contains(op))
                    {
                        instructions.Add(op);

                        ConsoleColor temp = Console.BackgroundColor;

                        Console.BackgroundColor = ConsoleColor.Red;

                        Console.WriteLine(VirtualMemoryManager.GetOpHex(Engine.PC));

                        Console.BackgroundColor = temp;
                    }

                    Engine.Step(1);
                }
            }
            else
            {
                Engine.Step((ulong)StepCount);
            }

            SyncDio();

            return Engine.PC;
        }

        public void SyncDio()
        {
            for (int i = 0; i < 32; i++)
            {
                Dios.Context.SetX(i,Engine.GetX(i));
                Dios.Context.SetQ(i,new Vector128<ulong>().WithElement(0,Engine.GetVector(i).d0).WithElement(1, Engine.GetVector(i).d1).AsSingle());
            }

            Dios.Context.tpidrro_el0 = Engine.tpidrro_el0;
            Dios.Context.NZCV = Engine.NZCV;
        }

        public void SyncUni()
        {
            for (int i = 0; i < 32; i++)
            {
                Engine.SetX(i,Dios.Context.GetX(i));

                Vector128<float> src = Dios.Context.GetQ(i);

                Engine.SetQ(i,&src);
            }

            Engine.tpidrro_el0 = Dios.Context.tpidrro_el0;
            Engine.NZCV = Dios.Context.NZCV;
        }

        public void CallSVC()
        {
            SyncDio();

            int id = (VirtualMemoryManager.ReadObject<int>(Engine.PC - 4) >> 5) & 0x7FFF;

            Dios.svc(id);

            SyncMemory();

            SyncUni();
        }

        public static ulong FallbackStepUni(ulong _context,ulong pc)
        {
            ExecutionContext* context = (ExecutionContext*)_context;

            ulong id = context->ID;

            return CpuThread.Threads[(int)id].ucf.StepUni(pc);
        }

        public void End()
        {
            uc.uc_emu_stop(Engine.context);
        }
    }
}
