using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.IPC;
using MejorNX.HLE.Horizon.Kernel.Objects;
using MejorNX.HLE.Horizon.Service.aud.Structs;
using Ryujinx.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.aud
{
    public class IAudioOut : ICommand
    {
        KSyncObject ReleaseEvent    { get; set; }
        IAalOutput Out              { get; set; }
        int Track                   { get; set; }

        public IAudioOut(KSyncObject ReleaseEvent, IAalOutput Out, int Track )
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1,  StartAudioOut},
                {4,  RegisterBufferEvent},
                {3,  AppendAudioOutBuffer},
                {5,  GetReleasedAudioOutBuffer }
            };

            this.ReleaseEvent = ReleaseEvent;
            this.Out = Out;
            this.Track = Track;
        }

        ulong StartAudioOut(ServiceCallContext context)
        {
            Out.Start(Track);

            return 0;
        }

        ulong RegisterBufferEvent(ServiceCallContext context)
        {
            uint Handle = ReleaseEvent.Handle;

            context.Response.HandleDescriptor = HandleDescriptor.MakeCopy(Handle);

            return 0;
        }

        ulong AppendAudioOutBuffer(ServiceCallContext context) => AppendAudioOutBufferImpl(context,context.Request.SendDescriptors[0].Address);

        ulong GetReleasedAudioOutBuffer(ServiceCallContext context)
        {
            ulong Position = context.Request.ReceiveDescriptors[0].Address;
            ulong Size = context.Request.ReceiveDescriptors[0].Size;

            return (ulong)GetReleasedAudioOutBufferImpl(context,(long)Position, (long)Size);
        }

        public long GetReleasedAudioOutBufferImpl(ServiceCallContext Context, long Position, long Size)
        {
            uint Count = (uint)((ulong)Size >> 3);

            long[] ReleasedBuffers = Out.GetReleasedBuffers(Track, (int)Count);

            MemoryWriter writer = VirtualMemoryManager.GetWriter();

            for (uint Index = 0; Index < Count; Index++)
            {
                long Tag = 0;

                if (Index < ReleasedBuffers.Length)
                {
                    Tag = ReleasedBuffers[Index];
                }

                writer.Seek((ulong)(Position + Index * 8));

                writer.WriteStruct(Tag);
            }

            Context.Writer.WriteStruct(ReleasedBuffers.Length);

            return 0;
        }

        ulong AppendAudioOutBufferImpl(ServiceCallContext context, ulong Position)
        {
            ulong Tag = context.Reader.ReadStruct<ulong>();

            MemoryReader Reader = VirtualMemoryManager.GetReader(Position);

            AudioOutData Data = Reader.ReadStruct<AudioOutData>();

            Reader.Seek((ulong)Data.SampleBufferPtr);

            byte[] Buffer = Reader.ReadArray<byte>((ulong)Data.SampleBufferSize);

            Out.AppendBuffer(Track,(long)Tag,Buffer);

            return 0;
        }
    }
}
