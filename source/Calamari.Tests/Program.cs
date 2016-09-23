using System.Reflection;
namespace Calamari.Tests
{
    public class Program
    {
        //This is a shell around Calamari.exe so we can use it in .net core testing, since in .net core when we reference the
        //Calamari project we only get the dll, not the exe
        public static int Main(string[] args)
        {
            if(args?.Length > 0)
            {
                System.Console.WriteLine("args are " + string.Join(" ", args));
                var program = new Calamari.Program("Calamari", typeof(Calamari.Program).GetTypeInfo().Assembly.GetInformationalVersion());
                return program.Execute(args);
            }

            var test = new Fixtures.PackageDownload.PackageDownloadFixture();
            test.ShouldFailWhenNoPackageId();
            return 0;
        }
    }
}