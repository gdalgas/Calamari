using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NuGet;

namespace Calamari.Util
{
    public static class CrossPlatform
    {
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

        public static string GetPackageExtension()
        {
#if USE_NUGET_V2_LIBS
            return Constants.PackageExtension;
#else
            return ".nupkg";
#endif
        }

        public static string GetManifestExtension()
        {
#if USE_NUGET_V2_LIBS
            return Constants.ManifestExtension;
#else
            return ".nuspec";
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if NET40
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }

        public static string GetCurrentDirectory()
        {
#if NET40
            return Environment.CurrentDirectory;
#else
            return System.IO.Directory.GetCurrentDirectory();
#endif
        }
    }
}