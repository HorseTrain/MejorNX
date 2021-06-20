using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MejorNX.HLE.VirtualFS
{
    public class FSContext : IDisposable
    {
        public static FSContext MainFSContext   { get; set; }

        public static string OperationPath      { get; set; }
        public static string SdPath             { get; set; }
        public static string SavePath           { get; set; }
        public Cart cart                        { get; set; }

        public FSContext()
        {
            MainFSContext = this;

            OperationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MejorNX";
            SdPath = OperationPath + "\\sd";
            SavePath = OperationPath + "\\save"; //TODO: Add game specific saves.

            InitDirectory(OperationPath);
            InitDirectory(SdPath);
        }

        void InitDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void OpenCart(string path)
        {
            cart = new Cart(path);
        }

        public void Dispose()
        {
            cart.Dispose();
        }
    }
}
