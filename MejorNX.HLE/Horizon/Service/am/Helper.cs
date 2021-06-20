using System;
using System.IO;

namespace MejorNX.HLE.Horizon.Service.am
{
    static unsafe class Helper
    {
        private const uint LaunchParamsMagic = 3348404170;

        public static byte[] MakeLaunchParams()
        {
            uint i = LaunchParamsMagic;

            byte* tmp = (byte*)&i;

            for (int p = 0; p < 4; p++)
            {
                Console.Write((char)tmp[p]);
            }

            //Size needs to be at least 0x88 bytes otherwise application errors.
            using (MemoryStream MS = new MemoryStream())
            {
                BinaryWriter Writer = new BinaryWriter(MS);

                MS.SetLength(0x88);

                Writer.Write(LaunchParamsMagic);
                Writer.Write(1);  //IsAccountSelected? Only lower 8 bits actually used.
                Writer.Write(1L); //User Id Low (note: User Id needs to be != 0)
                Writer.Write(0L); //User Id High

                return MS.ToArray();
            }
        }
    }
}
