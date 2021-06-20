using MejorNX.HLE;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MejorNX.Window
{
    public class GameScreen 
    {
        public const int Width = 1280;
        public const int Height = 720;

        public static GameScreen MainWindow     { get; set; }

        public GameWindow NativeWindow          { get; set; }

        public GameScreen()
        {
            new Thread(Open).Start();
        }

        void Open()
        {
            NativeWindow = new GameWindow(Width, Height);

            NativeWindow.UpdateFrame += UpdateFrame;

            NativeWindow.Run();
        }

        void UpdateFrame(object sender,FrameEventArgs args)
        {
            Switch.MainSwitch.ProcessFrame();

            NativeWindow.SwapBuffers();
        }
    }
}