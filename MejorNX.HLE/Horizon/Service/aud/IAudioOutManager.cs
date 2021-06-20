using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon.Kernel.Objects;
using Ryujinx.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.aud
{
    public class IAudioOutManager : ICommand
    {
        public IAudioOutManager()
        {
            Calls = new Dictionary<ulong, ServiceCall>()
            {
                {1, OpenAudioOut }
            };
        }

        ulong OpenAudioOut(ServiceCallContext context)
        {
            CallMethod(context, context.Request.SendDescriptors[0].Address, context.Request.SendDescriptors[0].Size, context.Request.ReceiveDescriptors[0].Address, context.Request.ReceiveDescriptors[0].Size);

            return 0;
        }

        void CallMethod(ServiceCallContext context, ulong SendPosition, ulong SendSize, ulong ReceivePosition, ulong Size)
        {
            IAalOutput AudioOut = Switch.MainSwitch.AudioOut;

            //NOTE: I could remove strings, and just use byte arrays.
            string Name = VirtualMemoryManager.GetReader(SendPosition).ReadString(SendSize);

            if (Name == string.Empty)
            {
                Name = "DeviceOut";
            }

            MemoryWriter writer = VirtualMemoryManager.GetWriter();

            byte[] DeviceNameBuffer = Encoding.ASCII.GetBytes(Name + "\0");

            if ((ulong)DeviceNameBuffer.Length <= (ulong)Size)
            {
                writer.Seek(ReceivePosition);

                writer.WriteStruct(DeviceNameBuffer);
            }
            else
            {
                throw new NotImplementedException();
            }

            int SampleRate = context.Reader.ReadStruct<int>();
            int Channels = context.Reader.ReadStruct<int>();

            Channels = (ushort)(Channels >> 16);

            if (SampleRate == 0)
            {
                SampleRate = 48000;
            }

            if (Channels < 1 || Channels > 2)
            {
                Channels = 2;
            }

            KSyncObject ReleaseEvent = new KSyncObject(context.process);

            ReleaseCallback Callback = () =>
            {
                ReleaseEvent.HostEvent.Set();
            };

            int Track = AudioOut.OpenTrack(SampleRate, Channels, Callback, out AudioFormat Format);

            context.Data = new IAudioOut(ReleaseEvent,AudioOut,Track);

            Helper.Make(context);

            context.Writer.WriteStruct(SampleRate);
            context.Writer.WriteStruct(Channels);
            
            //TODO:
            context.Writer.WriteStruct((int)Format);
            context.Writer.WriteStruct(1);//Filler?
        }
    }
}
