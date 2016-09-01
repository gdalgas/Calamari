using System.IO;
using Calamari.Commands;
using Calamari.Integration.Processes;
using Calamari.Util;
using NUnit.Framework;
using System.Reflection;

namespace Calamari.Tests.Fixtures
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [SetUp]
        public void AssertConfigurationFilesExist()
        {
            var calamariFullPath = typeof(DeployPackageCommand).GetTypeInfo().Assembly.FullLocalPath();
            var calamariConfigFilePath = calamariFullPath + ".config";
            if (!File.Exists(calamariConfigFilePath))
                throw new FileNotFoundException($"Unable to find {calamariConfigFilePath} which means the config file would not have been included in testing {calamariFullPath}");
        }
    }
}