using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MejorNX.HLE.Horizon.Service.vi
{
    static class Parcel
    {
        public static byte[] MakeIGraphicsBufferProducer(ulong BasePtr)
        {
            long Id = 0x20;
            long CookiePtr = 0L;

            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(2); 
                Writer.Write(0); 
                Writer.Write((int)(Id >> 0));
                Writer.Write((int)(Id >> 32));
                Writer.Write((int)(CookiePtr >> 0));
                Writer.Write((int)(CookiePtr >> 32));
                Writer.Write((byte)'d');
                Writer.Write((byte)'i');
                Writer.Write((byte)'s');
                Writer.Write((byte)'p');
                Writer.Write((byte)'d');
                Writer.Write((byte)'r');
                Writer.Write((byte)'v');
                Writer.Write((byte)'\0');
                Writer.Write(0L);

                return MakeParcel(MS.ToArray(), new byte[] { 0, 0, 0, 0 });
            }
        }

        public static byte[] MakeParcel(byte[] Data, byte[] Objs)
        {
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                Writer.Write(Data.Length);
                Writer.Write(0x10);
                Writer.Write(Objs.Length);
                Writer.Write(Data.Length + 0x10);

                Writer.Write(Data);
                Writer.Write(Objs);

                return MS.ToArray();
            }
        }

        public static byte[] GetParcelData(byte[] Parcel)
        {
            if (Parcel == null)
            {
                throw new ArgumentNullException(nameof(Parcel));
            }

            using (MemoryStream MS = new MemoryStream(Parcel))
            {
                BinaryReader Reader = new BinaryReader(MS);

                int DataSize = Reader.ReadInt32();
                int DataOffset = Reader.ReadInt32();
                int ObjsSize = Reader.ReadInt32();
                int ObjsOffset = Reader.ReadInt32();

                MS.Seek(DataOffset - 0x10, SeekOrigin.Current);

                return Reader.ReadBytes(DataSize);
            }
        }
    }
}
