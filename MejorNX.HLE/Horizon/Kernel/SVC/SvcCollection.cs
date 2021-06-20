using MejorNX.Common.Debugging;
using MejorNX.Cpu;
using MejorNX.Cpu.Memory;
using MejorNX.Cpu.Utilities.Tools;
using MejorNX.HLE.Horizon.Kernel.Objects;
using static MejorNX.Cpu.Memory.VirtualMemoryManager;
using static MejorNX.HLE.Horizon.Schedular;
using static MejorNX.HLE.Horizon.HorizonOS;

using System.Collections.Generic;
using System;
using MejorNX.HLE.Horizon.IPC;
using System.Threading;
using MejorNX.HLE.Horizon.Kernel.IPC.Execution;
using MejorNX.HLE.Horizon.Service;

namespace MejorNX.HLE.Horizon.Kernel.SVC
{
    public static class SvcCollection
    {
        //TODO: Make sure these are in order.
        public static Dictionary<int, SvcCall> Calls = new Dictionary<int, SvcCall>()
        {
            //Memory
            {0x01, SetHeapSize},
            {0x03, SetMemoryAttribute },
            {0x04, MapMemory },
            {0x05, UnmapMemory },
            {0x06, QueryMemory},

            //Threading
            {0x08, CreateThread},
            {0x09, StartThread },
            {0x0A, ExitThread },
            {0x0B, SleepThread},

            {0x0C, GetThreadPriority},
            {0x0D, SetThreadPriority },
            //{0x0F, SetThreadCoreMask },
            {0x13, MapSharedMemory },
            {0x15, CreateTransferMemory },
            {0x16, CloseHandle}, //?  
            
            //Thread Sync (Pain)
            {0x18, WaitSynchronization },
            {0x1A, ArbitrateLock },
            {0x1B, ArbitrateUnlock},
            {0x1C, WaitProcessWideKeyAtomic},
            {0x1E, GetSystemTick },
            {0x1D, SignalProcessWideKey },

            //IPC
            {0x1F, ConnectToNamedPort },
            {0x21, SendSyncRequest },
            {0x25, GetThreadId },
            {0x26, Break },
            {0x27, OutputDebugString},
            {0x29, GetInfo},

            {0x2C, MapPhysicalMemory }
        };

        public static void Call(CpuContext context, int call)
        {
            if (Calls.ContainsKey(call))
            {
                Calls[call](context);

                //Console.WriteLine($"Called SVC 0x{call.ToString("X")} {index}");
            }
            else
            {
                Debug.LogError($"Unknown SVC: 0x{StringTools.FillStringFront(call.ToString("X"), '0', 2)}", true);
            }
        }

        static void SvcMessage(object Message)
        {
            Debug.Log($"Svc: {Message}", LogLevel.High);
        }

        public static int GetTimeMs(ulong Ns)
        {
            ulong Ms = Ns / 1_000_000;

            if (Ms < int.MaxValue)
            {
                return (int)Ms;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public static void SetHeapSize(CpuContext context)
        {
            ulong Size = context.X[1];

            if (Size > CurrentHeapSize)
            {
                MainVMM.MapMemory(HeapBase, Size, MemoryPermission.ReadAndWrite, MemoryType.Heap, 0);
            }
            else
            {
                MainVMM.UnmapMemory(HeapBase + Size, CurrentHeapSize - Size, (int)MemoryType.Unmapped);
            }

            CurrentHeapSize = Size;

            context.X[0] = 0;
            context.X[1] = HeapBase;
        }

        public static void SetMemoryAttribute(CpuContext context)
        {
            ulong Position = context.X[0];
            ulong Size = context.X[1];
            int State0 = (int)context.X[2];
            int State1 = (int)context.X[3];

            if ((State0 == 0 && State1 == 0) || (State0 == 8 && State1 == 0))
            {
                throw new Exception();
            }
            else if (State0 == 8 && State1 == 8)
            {
                MainVMM.SetAttrBit(Position, Size, 3); //Is this always 3?
            }

            context.X[0] = 0;
        }

        public static void MapMemory(CpuContext context)
        {
            ulong Dest = context.X[0];
            ulong Source = context.X[1];
            ulong Size = context.X[2];

            MapInfo info = MainVMM.GetMapInfo(Source);

            MainVMM.MapMemory(Dest, Size, info.Permission, info.Type, info.Attr);

            context.X[0] = 0;
        }

        public static void UnmapMemory(CpuContext context)
        {
            ulong Dest = context.X[0];
            ulong Source = context.X[1];
            ulong Size = context.X[2];

            MapInfo DestInfo = MainVMM.GetMapInfo(Dest);

            MainVMM.UnmapMemory(Dest,Size,11);

            MainVMM.ReprotectMemory(Source,Size,DestInfo.Permission);

            MainVMM.ClearAttrBit(Source,Size,0);

            context.X[0] = 0;
        }

        public static void QueryMemory(CpuContext context)
        {
            ulong Destination = context.X[0];
            ulong QueryAddress = context.X[2];

            MapInfo ReigionData = MainVMM.GetMapInfo(QueryAddress);

            if (ReigionData == null)
            {
                ReigionData = new MapInfo(4294967296, 18446744069414584320,MemoryPermission.None,MemoryType.Reserved,0);
            }

            MemoryWriter writer = GetWriter();

            writer.WriteStruct(Destination, ReigionData.BaseAddress);
            writer.WriteStruct(Destination + 0x08, ReigionData.Size);
            writer.WriteStruct(Destination + 0x10, (uint)ReigionData.Type);
            writer.WriteStruct(Destination + 0x14, ReigionData.Attr); //Almost Always 0
            writer.WriteStruct(Destination + 0x18, (uint)ReigionData.Permission);
            writer.WriteStruct<uint>(Destination + 0x1c, 0);
            writer.WriteStruct<uint>(Destination + 0x20, 0);
            writer.WriteStruct<uint>(Destination + 0x24, 0);

            context.X[0] = 0;
            context.X[1] = 0;
        }

        public static void CreateThread(CpuContext context)
        {
            ulong Entry = context.X[1];
            ulong ArgsPointer = context.X[2];
            ulong StackTop = context.X[3];
            ulong Priority = context.X[4];
            ulong ProcessorId = context.X[5];

            KThread thread = GetThread(context.ThreadInformation);

            KThread NewThread = MainSched.MakeThread(thread.Process,Entry,StackTop,ArgsPointer, Priority, (int)ProcessorId);

            context.X[0] = 0;
            context.X[1] = NewThread.Handle;
        }

        public static void StartThread(CpuContext context)
        {
            ulong Handle = context.X[0];

            KThread thread = GetObject<KThread>(context.ThreadInformation, (uint)Handle);

            MainSched.DetatchAndExecuteKThread(thread);

            context.X[0] = 0;
        }

        public static void ExitThread(CpuContext context)
        {
            ((KThread)context.ThreadInformation).Cpu.StopExecution();
        }

        public static void SleepThread(CpuContext context)
        {
            ulong TimeSpan = context.X[0];

            Thread.Sleep(GetTimeMs(TimeSpan));

            context.X[0] = 0;
        }

        public static void GetThreadPriority(CpuContext context)
        {
            ulong Handle = context.X[1];

            KThread thread = GetObject<KThread>(context.ThreadInformation,(uint)Handle);

            context.X[0] = 0;
            context.X[1] = thread.ThreadPriority;
        }

        public static void SetThreadPriority(CpuContext context)
        {
            ulong ThreadHandle = context.X[0];
            ulong Priority = context.X[1];

            KThread thread = GetObject<KThread>(context.ThreadInformation,(uint)ThreadHandle);

            thread.ThreadPriority = Priority;

            context.X[0] = 0;
        }

        public static void SetThreadCoreMask(CpuContext context)
        {
            ulong ThreadHandle = context.X[0];
            ulong CoreMask = context.X[1];
            ulong CoreMask1 = context.X[2];

            context.X[0] = 0;
        }

        public static void MapSharedMemory(CpuContext context)
        {
            uint Handle = (uint)context.X[0];
            ulong Source = context.X[1];
            ulong Size = context.X[2];
            MemoryPermission permission = (MemoryPermission)context.X[3];

            KSharedMemory SharedMemory = GetObject<KSharedMemory>(context.ThreadInformation,Handle);

            MainVMM.MapMemory(Source,Size,permission,MemoryType.SharedMemory);

            MainVMM.FillZeros(Source,Size);

            SharedMemory.AddVirtualAddress(Source);

            context.X[0] = 0;
        }

        public static void CreateTransferMemory(CpuContext context)
        {
            ulong Source = context.X[1];
            ulong Size = context.X[2];
            ulong Perm = context.X[3];

            KThread thread = GetThread(context.ThreadInformation);

            MapInfo info = MainVMM.GetMapInfo(Source);

            MainVMM.ReprotectMemory(Source,Size,(MemoryPermission)Perm);

            KTransferMemory transfer = new KTransferMemory(thread.Process,Source,Size,(MemoryPermission)Perm);

            context.X[0] = 0;
            context.X[1] = transfer.Handle;
        }

        public static void CloseHandle(CpuContext context)
        {
            context.X[0] = 0;
        }

        public static void WaitSynchronization(CpuContext context)
        {
            ulong HandlePointer = context.X[1];
            ulong HandleCount = context.X[2];
            ulong TimeOut = context.X[3];

            WaitHandle[] Handles = GetThread(context.ThreadInformation).Process.GetEventHandles(HandlePointer,HandleCount);

            int HandleIndex;
            ulong Result = 0;

            if (TimeOut != ulong.MaxValue)
            {
                HandleIndex = WaitHandle.WaitAny(Handles, GetTimeMs(TimeOut));
            }
            else
            {
                HandleIndex = WaitHandle.WaitAny(Handles);
            }

            if (HandleIndex == WaitHandle.WaitTimeout)
            {
                //throw new NotImplementedException();
            }
            else if (HandleIndex == (int)HandleCount)
            {
                throw new NotImplementedException();
            }

            context.X[0] = Result;

            if (context.X[0] == 0)
            {
                context.X[1] = (ulong)HandleIndex;
            }
        }

        public static void ArbitrateLock(CpuContext context)
        {
            ulong Owner = context.X[0];
            ulong MutexAddress = context.X[1];
            ulong WaitThreadHandle = context.X[2];

            KThread thread = GetThread(context.ThreadInformation);
            KThread WaitThread = GetObject<KThread>(context.ThreadInformation, (uint)WaitThreadHandle);

            thread.Process.MutexLock(thread,WaitThread,(int)Owner, (int)WaitThreadHandle, MutexAddress);

            context.X[0] = 0;
        }

        public static void ArbitrateUnlock(CpuContext context)
        {
            ulong MutexAddress = context.X[0];

            KThread CurrentThread = GetThread(context.ThreadInformation);

            CurrentThread.Process.MutexUnlock(CurrentThread, MutexAddress);

            context.X[0] = 0;
        }

        public static void WaitProcessWideKeyAtomic(CpuContext context)
        {
            ulong MutexAddress = context.X[0];
            ulong CondVarAddress = context.X[1];
            ulong ThreadHandle = context.X[2];
            ulong TimeOut = context.X[3];

            GetThread(context.ThreadInformation).Process.CondVarWait(GetThread(context.ThreadInformation),ThreadHandle,MutexAddress,CondVarAddress,TimeOut);

            context.X[0] = 0;
        }

        public static void GetSystemTick(CpuContext context)
        {
            context.X[0] = 0;
        }

        public static void SignalProcessWideKey(CpuContext context)
        {
            ulong CondVarAddress = context.X[0];
            int Count = (int)context.X[1];

            KThread CurrentThread = (KThread)context.ThreadInformation;
            CurrentThread.Process.CondVarSignal(CurrentThread,CondVarAddress, Count);

            context.X[0] = 0;
        }

        public static void ConnectToNamedPort(CpuContext context)
        {
            ulong NamePointer = context.X[1];

            KThread thread = GetThread(context.ThreadInformation);

            string name = GetReader(NamePointer).ReadString();

            KSession session = new KSession(thread.Process, Factory.GetService(name), name);

            context.X[0] = 0;
            context.X[1] = session.Handle;
        }

        public static void SendSyncRequest(CpuContext context)
        {
            IPCHandler.SendSyncRequest(context,context.tpidrro_el0,0x100,(uint)context.X[0]);
        }

        public static void GetThreadId(CpuContext context)
        {
            KThread thread = GetThread(context.ThreadInformation);

            context.X[0] = 0;
            context.X[1] = 0;//thread.Handle;
        }

        public static void Break(CpuContext context)
        {
            Debug.LogError("Break LOL",true);

            context.X[0] = 0;
        }

        public static void OutputDebugString(CpuContext context)
        {
            ulong String = context.X[0];
            ulong Size = context.X[1];

            MemoryReader reader = GetReader();

            reader.Seek(String);

            string message = reader.ReadString(Size);

            SvcMessage(message);

            context.X[0] = 0;
        }

        public static void GetInfo(CpuContext context)
        {
            ulong InfoType = context.X[1];

            if (InfoType >= 18)
            {
                context.X[0] = 61441;

                return;
            }

            context.X[0] = 0;

            switch (InfoType)
            {
                case 0: context.X[1] = 0b1111; break;
                case 2: context.X[1] = 0x10000000; break;   
                case 3: context.X[1] = 0x20000000; break;   
                case 4: context.X[1] = 0x10000000 + 0x20000000; break;  
                case 5: context.X[1] = 0xCFEE0000; break;   
                case 6: context.X[1] = RamSize - ImageBase; break;
                case 7: context.X[1] = 806486016 + CurrentHeapSize; break;

                case 11: context.X[1] = 0; break;//(ulong)rng.Next() + (ulong)(rng.Next() << 32); break; //No Rng here :)
                case 12: context.X[1] = 0x8000000; break;
                case 13: context.X[1] = 4160749568; break; 
                case 14: context.X[1] = 0x10000000; break;  
                case 15: context.X[1] = 0x20000000; break; 
                case 16: context.X[1] = 1; break;

                default: Debug.ThrowNotImplementedException($"Info: {InfoType}"); break;
            }
        }

        static void MapPhysicalMemory(CpuContext context)
        {
            ulong Address = context.X[0];
            ulong Size = context.X[1];

            MainVMM.MapMemory(Address,Size,MemoryPermission.ReadAndWrite,MemoryType.Heap);

            context.X[0] = 0;
        }
    }
}
