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
    //This is 1-1 with ryujinx. 
    //TODO: Learn what this is fucking doing.
    public partial class Process
    {
        private const int MutexHasListenersMask = 0x40000000;

        public void SleepThread(KThread thread)
        {
            //Debug.Log($"Sleeping Thread {thread.Handle}");

            thread.Wait();

            //Debug.Log($"Waking Thread {thread.Handle}");
        }

        public void WakeThread(KThread thread)
        {
            thread.Send();
        }

        //Threading
        public WaitHandle[] GetEventHandles(ulong Pointer, ulong Count)
        {
            MemoryReader reader = GetReader(Pointer);

            List<WaitHandle> Handles = new List<WaitHandle>();

            for (ulong i = 0; i < Count; i++)
            {
                Handles.Add(((KSyncObject)SyncHandles[reader.ReadStruct<uint>()]).HostEvent);
            }

            return Handles.ToArray();
        }

        private KThread PopCondVarThreadUnsafe(ulong CondVarAddress)
        {
            KThread WakeThread = null;

            foreach (KThread Thread in ThreadArbiterList)
            {
                if (Thread.CondVarAddress == CondVarAddress)
                {
                    WakeThread = Thread;

                    break;
                }

                if (WakeThread == null) //|| Thread.ActualPriority < WakeThread.ActualPriority
                {

                }
            }

            if (WakeThread != null)
            {
                ThreadArbiterList.Remove(WakeThread);
            }

            return WakeThread;
        }

        private (KThread, int) PopMutexThreadUnsafe(KThread OwnerThread, ulong MutexAddress)
        {
            int Count = 0;

            KThread WakeThread = null;

            foreach (KThread Thread in OwnerThread.MutexWaiters)
            {
                if (Thread.MutexAddress != MutexAddress)
                {
                    continue;
                }

                if (WakeThread == null) // || Thread.ActualPriority < WakeThread.ActualPriority
                {
                    WakeThread = Thread;
                }

                Count++;
            }

            if (WakeThread != null)
            {
                OwnerThread.MutexWaiters.Remove(WakeThread);
            }

            return (WakeThread, Count);
        }

        private void InsertWaitingMutexThreadUnsafe(KThread OwnerThread, KThread WaitThread)
        {
            WaitThread.MutexOwner = OwnerThread;

            if (!OwnerThread.MutexWaiters.Contains(WaitThread))
            {
                OwnerThread.MutexWaiters.Add(WaitThread);

                //OwnerThread.UpdatePriority();
            }
        }

        private void InsertWaitingMutexThreadUnsafe(int OwnerThreadHandle, KThread WaitThread)
        {
            KThread OwnerThread = (KThread)WaitThread.Process.ServiceHandles.GetObject((uint)OwnerThreadHandle);

            if (OwnerThread == null)
            {
                return;
            }

            InsertWaitingMutexThreadUnsafe(OwnerThread, WaitThread);
        }

        private void UpdateMutexOwnerUnsafe(KThread CurrThread, KThread NewOwner, ulong MutexAddress)
        {
            //Go through all threads waiting for the mutex,
            //and update the MutexOwner field to point to the new owner.
            for (int Index = 0; Index < CurrThread.MutexWaiters.Count; Index++)
            {
                KThread Thread = CurrThread.MutexWaiters[Index];

                if (Thread.MutexAddress == MutexAddress)
                {
                    CurrThread.MutexWaiters.RemoveAt(Index--);

                    InsertWaitingMutexThreadUnsafe(NewOwner, Thread);
                }
            }
        }

        public void MutexUnlock(KThread CurrThread, ulong MutexAddress)
        {
            lock (ThreadSyncLock)
            {
                (KThread OwnerThread, int Count) = PopMutexThreadUnsafe(CurrThread, MutexAddress);

                if (OwnerThread == CurrThread)
                {
                    throw new InvalidOperationException();
                }

                if (OwnerThread != null)
                {
                    //Remove all waiting mutex from the old owner,
                    //and insert then on the new owner.
                    UpdateMutexOwnerUnsafe(CurrThread, OwnerThread, MutexAddress);

                    //CurrThread.UpdatePriority();

                    int HasListeners = Count >= 2 ? MutexHasListenersMask : 0;

                    CurrThread.Cpu.WriteIntToSharedAddress(MutexAddress, HasListeners | (int)OwnerThread.WaitHandle);

                    OwnerThread.WaitHandle = 0;
                    OwnerThread.MutexAddress = 0;
                    OwnerThread.CondVarAddress = 0;
                    OwnerThread.MutexOwner = null;

                    //OwnerThread.UpdatePriority();

                    WakeThread(OwnerThread);
                }
                else
                {
                    CurrThread.Cpu.WriteIntToSharedAddress(MutexAddress, 0);
                }
            }
        }

        public void MutexLock(KThread Current, KThread Wait, int OwnerHandle, int WaitThreadHandle, ulong MutexAddress)
        {
            lock (ThreadSyncLock)
            {
                MemoryReader reader = GetReader(MutexAddress);

                int MutexValue = reader.ReadStruct<int>();

                //Console.WriteLine(MutexAddress);

                if (MutexValue != (OwnerHandle | MutexHasListenersMask))
                {
                    return;
                }

                Current.WaitHandle = (uint)WaitThreadHandle;
                Current.MutexAddress = MutexAddress;

                InsertWaitingMutexThreadUnsafe(OwnerHandle, Wait);
            }

            SleepThread(Wait);
        }

        public void CondVarWait(KThread wait, ulong WaitThreadHandle, ulong MutexAddress, ulong CondVarAddress, ulong TimeOut)
        {
            wait.WaitHandle = WaitThreadHandle;
            wait.MutexAddress = MutexAddress;
            wait.CondVarAddress = CondVarAddress;

            lock (ThreadSyncLock)
            {
                MutexUnlock(wait,MutexAddress);

                wait.CondVarSignaled = false;

                ThreadArbiterList.Add(wait);
            }

            if (TimeOut != ulong.MaxValue)
            {
                throw new NotImplementedException();
            }
            else
            {
                SleepThread(wait);
            }
        }

        public void CondVarSignal(KThread thread, ulong Address, int Count)
        {
            lock (ThreadSyncLock)
            {
                while (Count == -1 || Count-- > 0)
                {
                    KThread WaitThread = PopCondVarThreadUnsafe(Address);

                    if (WaitThread == null)
                    {
                        break;
                    }

                    WaitThread.CondVarSignaled = true;

                    ulong MutexAddress = WaitThread.MutexAddress;

                    thread.Cpu.SetExclusive(MutexAddress);

                    int MutexValue = GetReader(MutexAddress).ReadStruct<int>();

                    MemoryWriter writer = GetWriter(MutexAddress);
                    MemoryReader reader = GetReader();

                    while (MutexValue != 0)
                    {
                        if (thread.Cpu.TestExclusive(MutexAddress))
                        {
                            InsertWaitingMutexThreadUnsafe(MutexValue & ~MutexHasListenersMask, WaitThread);

                            writer.Seek(MutexAddress);

                            writer.WriteStruct(MutexValue | MutexHasListenersMask);

                            thread.Cpu.ClearExclusive();

                            break;
                        }

                        thread.Cpu.SetExclusive(MutexAddress);

                        reader.Seek(MutexAddress);
                        MutexValue = reader.ReadStruct<int>();
                    }

                    if (MutexValue == 0)
                    {
                        thread.Cpu.WriteIntToSharedAddress(MutexAddress,(int)WaitThread.WaitHandle);

                        WaitThread.WaitHandle = 0;
                        WaitThread.MutexAddress = 0;
                        WaitThread.CondVarAddress = 0;

                        WaitThread.MutexOwner = null;

                        WakeThread(WaitThread);
                    }
                }
            }
        }
    }
}
