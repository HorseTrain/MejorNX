using MejorNX.Common.Debugging;
using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.Maxwell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MejorNX.HLE.Horizon.Service.vi
{
    public unsafe class NvFlinger 
    {
        public MaxwellContext Renderer          { get; set; }
        public KSyncObject ReleaseEvent         { get; set; }
        public ManualResetEvent WaitBufferFree  { get; set; }
        bool Disposed                           { get; set; }

        Dictionary<(string, int), ServiceProcessParcel> Commands { get; set; }

        [Flags]
        private enum HalTransform
        {
            FlipX = 1 << 0,
            FlipY = 1 << 1,
            Rotate90 = 1 << 2
        }

        private enum BufferState
        {
            Free,
            Dequeued,
            Queued,
            Acquired
        }

        private struct Rect
        {
            public int Top;
            public int Left;
            public int Right;
            public int Bottom;
        }

        private struct BufferEntry
        {
            public BufferState State;

            public HalTransform Transform;

            public Rect Crop;

            public GbpBuffer Data;
        }

        private BufferEntry[] BufferQueue;

        public NvFlinger(MaxwellContext Renderer, KSyncObject ReleaseEvent)
        {
            this.Renderer = Renderer;

            Commands = new Dictionary<(string, int), ServiceProcessParcel>()
            {
                 { ("android.gui.IGraphicBufferProducer", 0x1), GbpRequestBuffer    },
                 { ("android.gui.IGraphicBufferProducer", 0x9), GbpQuery            },
                 { ("android.gui.IGraphicBufferProducer", 0xa), GbpConnect          },
                 { ("android.gui.IGraphicBufferProducer", 0xe), GbpPreallocBuffer   },
                 { ("android.gui.IGraphicBufferProducer", 0x3), GbpDequeueBuffer    },
                 { ("android.gui.IGraphicBufferProducer", 0x7), GbpQueueBuffer      },
            };

            BufferQueue = new BufferEntry[64];

            this.ReleaseEvent = ReleaseEvent;

            WaitBufferFree = new ManualResetEvent(false);
        }

        ulong GbpRequestBuffer(ServiceCallContext context, MemoryReader reader)
        {
            int Slot = reader.ReadStruct<int>();

            BinaryBuilder builder = new BinaryBuilder();

            BufferEntry Entry = BufferQueue[Slot];

            ulong BufferSize = (ulong)Entry.Data.Size;

            builder.WriteStruct(1);
            builder.WriteStruct(BufferSize);

            Entry.Data.Write(builder);

            builder.WriteStruct(0);

            return MakeReplyParcel(context, builder.GetBuffer());
        }

        ulong GbpQuery(ServiceCallContext context, MemoryReader reader) => MakeReplyParcel(context, 0,0);
        ulong GbpConnect(ServiceCallContext context,MemoryReader reader) => MakeReplyParcel(context, 1280, 720, 0, 0, 0);

        ulong GbpPreallocBuffer(ServiceCallContext context, MemoryReader reader)
        {
            int Slot = reader.ReadStruct<int>();

            int BufferCount = reader.ReadStruct<int>();

            if (BufferCount > 0)
            {
                ulong BufferSize = reader.ReadStruct<ulong>();

                BufferQueue[Slot].State = BufferState.Free;

                BufferQueue[Slot].Data = new GbpBuffer(reader);
            }

            return MakeReplyParcel(context,0);
        }

        ulong GbpDequeueBuffer(ServiceCallContext context, MemoryReader reader)
        {
            int Format = reader.ReadStruct<int>();
            int Width = reader.ReadStruct<int>();
            int Height = reader.ReadStruct<int>();
            int GetTimestamps = reader.ReadStruct<int>();
            int Usage = reader.ReadStruct<int>();

            int Slot = GetFreeSlotBlocking(Width,Height);

            return MakeReplyParcel(context,Slot, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        ulong GbpQueueBuffer(ServiceCallContext context, MemoryReader reader)
        {
            int Slot = reader.ReadStruct<int>();
            int Unknown4 = reader.ReadStruct<int>();
            int Unknown8 = reader.ReadStruct<int>();
            int Unknownc = reader.ReadStruct<int>();
            int Timestamp = reader.ReadStruct<int>();
            int IsAutoTimestamp = reader.ReadStruct<int>();
            int CropTop = reader.ReadStruct<int>();
            int CropLeft = reader.ReadStruct<int>();
            int CropRight = reader.ReadStruct<int>();
            int CropBottom = reader.ReadStruct<int>();
            int ScalingMode = reader.ReadStruct<int>();
            int Transform = reader.ReadStruct<int>();
            int StickyTransform = reader.ReadStruct<int>();
            int Unknown34 = reader.ReadStruct<int>();
            int Unknown38 = reader.ReadStruct<int>();
            int IsFenceValid = reader.ReadStruct<int>();
            int Fence0Id = reader.ReadStruct<int>();
            int Fence0Value = reader.ReadStruct<int>();
            int Fence1Id = reader.ReadStruct<int>();
            int Fence1Value = reader.ReadStruct<int>();

            BufferQueue[Slot].Transform = (HalTransform)Transform;

            BufferQueue[Slot].Crop.Top = CropTop;
            BufferQueue[Slot].Crop.Left = CropLeft;
            BufferQueue[Slot].Crop.Right = CropRight;
            BufferQueue[Slot].Crop.Bottom = CropBottom;

            BufferQueue[Slot].State = BufferState.Queued;

            ReleaseBuffer(Slot);

            return MakeReplyParcel(context, 1280, 720, 0, 0, 0);
        }

        void ReleaseBuffer(int Slot)
        {
            BufferQueue[Slot].State = BufferState.Free;

            ReleaseEvent.HostEvent.Set();

            lock (WaitBufferFree)
            {
                WaitBufferFree.Set();
            }
        }

        public ulong ProcessParcelRequest(ServiceCallContext context, byte[] ParcelData, int CommandID)
        {
            GCHandle handle = GCHandle.Alloc(ParcelData,GCHandleType.Pinned);

            MemoryReader reader = new MemoryReader(MemoryTools.GetPointer(ParcelData));

            reader.Seek(4);

            int StringSize = reader.ReadStruct<int>();

            string InterfaceName = reader.ReadString((ulong)StringSize,true);

            /* This is in ryujinx, but it seems redundant.
            ulong Padding = reader.Location & 0xf;

            if (Padding != 0)
            {
                reader.Advance(0x10 - Padding);
            }
            */

            handle.Free();

            reader.Seek(0x50);

            ServiceProcessParcel command;

            Commands.TryGetValue((InterfaceName,CommandID),out command);

            if (command != null)
            {
                return command(context,reader);
            }

            Debug.ThrowNotImplementedException($"{InterfaceName} 0x{CommandID.ToString("X")}");

            return 0;
        }

        ulong MakeReplyParcel(ServiceCallContext context, params int[] data)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                foreach (int Int in data)
                {
                    Writer.Write(Int);
                }

                return MakeReplyParcel(context, MS.ToArray());
            }
        }

        ulong MakeReplyParcel(ServiceCallContext Context, byte[] Data)
        {
            ulong ReplyPos = Context.Request.ReceiveDescriptors[0].Address;
            ulong ReplySize = Context.Request.ReceiveDescriptors[0].Size;

            byte[] Reply = Parcel.MakeParcel(Data, new byte[0]);

            VirtualMemoryManager.GetWriter(ReplyPos).WriteStruct(Reply);

            return 0;
        }

        int GetFreeSlotBlocking(int Width, int Height)
        {
            int Slot;

            do
            {
                lock (WaitBufferFree)
                {
                    if ((Slot = GetFreeSlot(Width, Height)) != -1)
                    {
                        break;
                    }

                    if (Disposed)
                    {
                        break;
                    }

                    WaitBufferFree.Reset();
                }

                WaitBufferFree.WaitOne();
            }
            while (!Disposed);

            return Slot;
        }

        private int GetFreeSlot(int Width, int Height)
        {
            lock (BufferQueue)
            {
                for (int Slot = 0; Slot < BufferQueue.Length; Slot++)
                {
                    if (BufferQueue[Slot].State != BufferState.Free)
                    {
                        continue;
                    }

                    GbpBuffer Data = BufferQueue[Slot].Data;

                    if (Data.Width == Width &&
                        Data.Height == Height)
                    {
                        BufferQueue[Slot].State = BufferState.Dequeued;

                        return Slot;
                    }
                }
            }

            return -1;
        }
    }
}
