using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Calamari.Util
{
    public static class CrossPlatform
    {
        public static string GetCurrentDirectory()
        {
#if USE_SYSTEM_IO_DIRECTORY
            return System.IO.Directory.GetCurrentDirectory();
#else
            return Environment.CurrentDirectory;
#endif
        }

        public static string GetApplicationTempDir()
        {
#if NET40
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#else
            var path = Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? Environment.GetEnvironmentVariable("TMPDIR") ?? "/tmp";
#endif
            path = Path.Combine(path, Assembly.GetEntryAssembly().GetName().Name);
            path = Path.Combine(path, "Temp");
            return path;
        }

        public static Encoding GetDefaultEncoding()
        {
#if HAS_DEFAULT_ENCODING
            return Encoding.Default;
#else
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Encoding.GetEncoding("windows-1251") : Encoding.UTF8;
#endif
        }

        public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
#if NET40
            File.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
#else
            File.Move(destinationFileName, destinationBackupFileName);
            File.Move(sourceFileName, destinationFileName);
#endif
        }
    }
}