using System.IO;
using Calamari.Util;

namespace Calamari.Integration.Processes
{
    public static class EmbeddedResource
    {
        public static string ReadEmbeddedText(string name)
        {
            var thisType = typeof(EmbeddedResource);
            using (var resource = thisType.GetAssembly().GetManifestResourceStream(name))
            using (var reader = new StreamReader(resource))
            {
                return reader.ReadToEnd();
            }
        }
    }
}