using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Calamari.Integration.EmbeddedResources
{
    public static class AssemblyEmbeddedResourceExtensions
    {
        public static IEnumerable<string> GetEmbeddedResourceNames(this Assembly assembly)
        {
            return assembly.GetManifestResourceNames();
        }

        public static string GetEmbeddedResourceText(this Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}