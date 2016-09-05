using System;
using System.IO;
using Calamari.Commands;
using Calamari.Integration.Processes;
using Calamari.Integration.ServiceMessages;
using Octostache;
using System.Reflection;
#if APPROVAL_TESTS
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
#endif

namespace Calamari.Tests.Helpers
{
#if APPROVAL_TESTS
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("Approved")]
#endif
    public abstract class CalamariFixture
    {
        protected CommandLine Calamari()
        {
#if NET40
            var calamariFullPath = typeof (DeployPackageCommand).GetTypeInfo().Assembly.FullLocalPath();
#else
            var folder = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.FullLocalPath());
            var calamariFullPath = Path.Combine(folder, "Calamari.Tests.exe");
#endif
            return CommandLine.Execute(calamariFullPath);
        }

        protected CommandLine OctoDiff()
        {
            var octoDiffExe = ApplyDeltaCommand.FindOctoDiffExecutable();

            return CommandLine.Execute(octoDiffExe);
        }

        protected CalamariResult Invoke(CommandLine command, VariableDictionary variables)
        {
            var capture = new CaptureCommandOutput();
            var runner = new CommandLineRunner(new SplitCommandOutput(new ConsoleCommandOutput(), new ServiceMessageCommandOutput(variables), capture));
            var result = runner.Execute(command.Build());
            return new CalamariResult(result.ExitCode, capture);
        }

        protected CalamariResult Invoke(CommandLine command)
        {
            return Invoke(command, new VariableDictionary());
        }

        protected string GetFixtureResouce(params string[] paths)
        {
            var path = GetType().Namespace.Replace("Calamari.Tests.", String.Empty);
            path = path.Replace('.', Path.DirectorySeparatorChar);
            return Path.Combine(TestEnvironment.CurrentWorkingDirectory, path, Path.Combine(paths));
        }
    }
}