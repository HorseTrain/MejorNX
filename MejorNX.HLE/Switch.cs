using MejorNX.Cpu.Memory;
using MejorNX.HLE.Horizon;
using MejorNX.HLE.IO;
using MejorNX.HLE.VirtualFS;
using MejorNX.Maxwell;
using OpenTK.Graphics.ES20;
using Ryujinx.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MejorNX.HLE
{
    public unsafe class Switch 
    {
        public static Switch MainSwitch     { get; set; }
        public OpenALAudioOut AudioOut      { get; set; } //For now, i'll just use an old version of ryujinx's audio engine.

        public HorizonOS Hos                { get; set; }
        public FSContext VirtualFS          { get; set; }
        public MaxwellContext Gpu           { get; set; }

        bool Ready                          { get; set; }

        public Switch()
        {
            MainSwitch = this;

            Cpu.CpuContext.InitCPU();

            LocalFile.InitFileSystem();

            AudioOut = new OpenALAudioOut();

            Hos = new HorizonOS();
            VirtualFS = new FSContext();
            Gpu = new MaxwellContext(VirtualMemoryManager.BaseAddress);

            Ready = true;
        }

        //int press = 100;

        private const int HidHeaderSize = 0x400;
        private const int HidTouchScreenSize = 0x3000;
        private const int HidMouseSize = 0x400;
        private const int HidKeyboardSize = 0x400;
        private const int HidUnkSection1Size = 0x400;
        private const int HidUnkSection2Size = 0x400;
        private const int HidUnkSection3Size = 0x400;
        private const int HidUnkSection4Size = 0x400;
        private const int HidUnkSection5Size = 0x200;
        private const int HidUnkSection6Size = 0x200;
        private const int HidUnkSection7Size = 0x200;
        private const int HidUnkSection8Size = 0x800;
        private const int HidControllerSerialsSize = 0x4000;
        private const int HidControllersSize = 0x32000;
        private const int HidUnkSection9Size = 0x800;

        private const int HidTouchHeaderSize = 0x28;
        private const int HidTouchEntrySize = 0x298;

        private const int HidTouchEntryHeaderSize = 0x10;
        private const int HidTouchEntryTouchSize = 0x28;

        private const int HidControllerSize = 0x5000;
        private const int HidControllerHeaderSize = 0x28;
        private const int HidControllerLayoutsSize = 0x350;

        private const int HidControllersLayoutHeaderSize = 0x20;
        private const int HidControllersInputEntrySize = 0x30;

        private const int HidHeaderOffset = 0;
        private const int HidTouchScreenOffset = HidHeaderOffset + HidHeaderSize;
        private const int HidMouseOffset = HidTouchScreenOffset + HidTouchScreenSize;
        private const int HidKeyboardOffset = HidMouseOffset + HidMouseSize;
        private const int HidUnkSection1Offset = HidKeyboardOffset + HidKeyboardSize;
        private const int HidUnkSection2Offset = HidUnkSection1Offset + HidUnkSection1Size;
        private const int HidUnkSection3Offset = HidUnkSection2Offset + HidUnkSection2Size;
        private const int HidUnkSection4Offset = HidUnkSection3Offset + HidUnkSection3Size;
        private const int HidUnkSection5Offset = HidUnkSection4Offset + HidUnkSection4Size;
        private const int HidUnkSection6Offset = HidUnkSection5Offset + HidUnkSection5Size;
        private const int HidUnkSection7Offset = HidUnkSection6Offset + HidUnkSection6Size;
        private const int HidUnkSection8Offset = HidUnkSection7Offset + HidUnkSection7Size;
        private const int HidControllerSerialsOffset = HidUnkSection8Offset + HidUnkSection8Size;
        private const int HidControllersOffset = HidControllerSerialsOffset + HidControllerSerialsSize;
        private const int HidUnkSection9Offset = HidControllersOffset + HidControllersSize;

        public void ProcessFrame()
        {
            if (Ready)
            {
                Gpu.ProcessFrame();
            }
        }
    }
}
