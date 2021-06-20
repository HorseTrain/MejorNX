using MejorNX.Common.Debugging;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Loaders;
using System;
using System.Collections.Generic;
using static MejorNX.Cpu.Memory.VirtualMemoryManager;
using MejorNX.Cpu.Memory;
using System.IO;
using MejorNX.HLE.Horizon.Service.am;
using MejorNX.Common.Utilities;
using System.Threading;

namespace MejorNX.HLE.Horizon
{
    public unsafe partial class Process 
    {
        List<IExecutable> Executables           { get; set; }
        public ulong ImageBase                  { get; set; }
        public ulong ProgramBase                { get; set; }
        public AppletManager AppletManager      { get; set; }

        public ObjectCollection ServiceHandles  { get; set; }
        public ObjectCollection SyncHandles     { get; set; }

        public List<KThread> ThreadArbiterList = new List<KThread>();
        object ThreadSyncLock                   { get; set; }

        public Process()
        {
            ServiceHandles = new ObjectCollection();
            SyncHandles = ServiceHandles;

            Executables = new List<IExecutable>();

            ImageBase = VirtualMemoryManager.ImageBase;
            ProgramBase = ImageBase;

            AppletManager = new AppletManager(this);
            AppletManager.SetFocus(true);

            ThreadArbiterList = new List<KThread>();
            ThreadSyncLock = new object();
        }

        public void AddExecutable(IExecutable executable) => Executables.Add(executable);

        public void LoadCart(string path)
        {
            if (!path.EndsWith("\\"))
                path += "\\";

            void LoadNSO(string name)
            {
                string rpath = path + name;

                if (File.Exists(rpath))
                {
                    Debug.Log($"Uploaded NSO {name}");

                    AddExecutable(LoadExecutable(rpath,true));
                }
            }

            LoadNSO("rtld");

            ImageBase += PageSize;

            LoadNSO("main");

            for (int i = 0; i < 10; i++)
                LoadNSO($"subsdk{i}");

            LoadNSO("sdk");

            Switch.MainSwitch.VirtualFS.OpenCart(path + "main.romfs");
        }

        public void LoadHomebrew(string path)
        {
            AddExecutable(LoadExecutable(path));
        }

        public void UploadExecutable(IExecutable executable)
        {
            MemoryWriter Writer = GetWriter();

            ulong Addr = Allocator.Allocate(executable.Data.Offset + executable.Data.Length);

            Writer.WriteStruct(ImageBase + executable.Text.Offset,executable.Text.Data);
            Writer.WriteStruct(ImageBase + executable.RoData.Offset, executable.RoData.Data);
            Writer.WriteStruct(ImageBase + executable.Data.Offset, executable.Data.Data);

            MainVMM.MapMemory(ImageBase + executable.Text.Offset, executable.Text.Length, MemoryPermission.ReadAndExecute, MemoryType.CodeStatic);
            MainVMM.MapMemory(ImageBase + executable.RoData.Offset, executable.RoData.Length, MemoryPermission.Read, MemoryType.CodeMutable);
            MainVMM.MapMemory(ImageBase + executable.Data.Offset, executable.Data.Length, MemoryPermission.ReadAndWrite, MemoryType.CodeMutable);

            if (executable.Mod0Offset == 0)
            {
                ulong BssOffset = executable.Data.Offset + executable.Data.Length;
                ulong BssSize = executable.BssSize;

                MainVMM.MapMemory(ImageBase + BssOffset, BssSize, MemoryPermission.ReadAndWrite, MemoryType.Normal);

                ImageBase = ImageBase + BssOffset + BssSize;

                return;
            }

            MemoryReader reader = GetReader();

            ulong Mod0Offset = ImageBase + executable.Mod0Offset;

            ulong DynamicOffset = Mod0Offset + reader.ReadStructAtOffset<uint>(Mod0Offset + 4);
            ulong BssStartOffset = Mod0Offset + reader.ReadStructAtOffset<uint>(Mod0Offset + 8);
            ulong BssEnd = Mod0Offset + reader.ReadStructAtOffset<uint>(Mod0Offset + 12);

            MainVMM.MapMemory(BssStartOffset, BssEnd - BssStartOffset, MemoryPermission.ReadAndWrite, MemoryType.Normal);

            ImageBase = PageRoundUp(BssEnd);
        }

        public IExecutable LoadExecutable(string path, bool forcenso = false)
        {
            byte[] Program = IO.LocalFile.ReadFileBytes(path);

            IExecutable Out;

            if (path.EndsWith(".nso") || forcenso)
            {
                Out = new NsoExecutable(Program);
            }
            else if (path.EndsWith(".nro"))
            {
                Out = new NroExecutable(Program);
            }    
            else
            {
                Debug.LogError($"{path} Unknown File.",true);

                return null;
            }

            UploadExecutable(Out);

            return Out;
        }

        public void StartProgram()
        {
            KThread EntryThread = Schedular.MainSched.MakeThread(this,ProgramBase,StackTop,0,44,0);

            //EntryThread.Cpu.Execute();

            Schedular.MainSched.DetatchAndExecuteKThread(EntryThread);
        }
    }
}
