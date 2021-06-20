using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MejorNX.HLE.IO
{
    public static class LocalFile
    {
        public static string ExternalPath => "";
        public static string OperationPath => ExternalPath + "";
        public static string RomPath => OperationPath + "";

        public static string GetDir(string path)
        {
            return OperationPath + path;
        }

        public static void InitFileSystem()
        {
            CreateDir(OperationPath);
            CreateDir(RomPath);
        }

        static void CreateDir(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetOperationPath(string path) => OperationPath + path;

        public static byte[] ReadFileBytes(string path) => File.ReadAllBytes(GetOperationPath(path));
    }
}
