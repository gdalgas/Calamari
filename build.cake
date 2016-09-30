//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&prerelease"
#addin "MagicChunks"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var testFilter = Argument("where", "");
var framework = Argument("framework", "");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var artifactsDir = "./artifacts";
var globalAssemblyFile = "./Calamari/Properties/AssemblyInfo.cs";
var projectsToPackage = new []{"./source/Calamari", "./source/Calamari.Azure"};

var isContinuousIntegrationBuild = !BuildSystem.IsLocalBuild;

var gitVersionInfo = GitVersion(new GitVersionSettings {
    OutputType = GitVersionOutput.Json
});

var nugetVersion = isContinuousIntegrationBuild ? gitVersionInfo.NuGetVersion : "0.0.0";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    Information("Building Calamari v{0}", nugetVersion);
});

Teardown(context =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("__UpdateProjectJsonVersion")
    .IsDependentOn("__Pack");
    //.IsDependentOn("__Publish");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

Task("Restore")
    .Does(() => DotNetCoreRestore());

Task("__UpdateAssemblyVersionInformation")
    .WithCriteria(isContinuousIntegrationBuild)
    .Does(() =>
{
     GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        UpdateAssemblyInfoFilePath = globalAssemblyFile
    });

    Information("AssemblyVersion -> {0}", gitVersionInfo.AssemblySemVer);
    Information("AssemblyFileVersion -> {0}", $"{gitVersionInfo.MajorMinorPatch}.0");
    Information("AssemblyInformationalVersion -> {0}", gitVersionInfo.InformationalVersion);
});

Task("Build")
    .Does(() =>
{
    var settings =  new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    if(!string.IsNullOrEmpty(framework))
        settings.Framework = framework;      

    DotNetCoreBuild("**/project.json", settings);
});

Task("Test")
    .Does(() =>
{
    var settings =  new DotNetCoreTestSettings
    {
        Configuration = configuration
    };

    if(string.IsNullOrEmpty(framework) || framework != "netcoreapp1.0")
        DotNetCoreTest("source/Calamari.Azure.Tests/project.json", settings);

    if(!string.IsNullOrEmpty(framework))
        settings.Framework = framework;   

    DotNetCoreTest("source/Calamari.Tests/project.json", settings);
});

Task("__UpdateProjectJsonVersion")
    .WithCriteria(isContinuousIntegrationBuild)
    .Does(() =>
{
    foreach(var projectToPackage in projectsToPackage)
    {
        var projectToPackagePackageJson = $"{projectToPackage}/project.json";
        Information("Updating {0} version -> {1}", projectToPackagePackageJson, nugetVersion);

        TransformConfig(projectToPackagePackageJson, projectToPackagePackageJson, new TransformationCollection {
            { "version", nugetVersion }
        });
    };
});

Task("__Pack")
    .Does(() =>
{
    foreach(var projectToPackage in projectsToPackage)
    {
        DotNetCorePack(projectToPackage, new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDir,
            NoBuild = true
        });
    };
});

Task("__Publish")
    .WithCriteria(isContinuousIntegrationBuild)
    .Does(() =>
{
    var isPullRequest = !String.IsNullOrEmpty(EnvironmentVariable("APPVEYOR_PULL_REQUEST_NUMBER"));
    var isMasterBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && !isPullRequest;
    var shouldPushToMyGet = !BuildSystem.IsLocalBuild;
    var shouldPushToNuGet = !BuildSystem.IsLocalBuild && isMasterBranch;

    if (shouldPushToMyGet)
    {
        NuGetPush("artifacts/Calamari." + nugetVersion + ".nupkg", new NuGetPushSettings {
            Source = "https://octopus.myget.org/F/octopus-dependencies/api/v3/index.json",
            ApiKey = EnvironmentVariable("MyGetApiKey")
        });
        NuGetPush("artifacts/Calamari." + nugetVersion + ".symbols.nupkg", new NuGetPushSettings {
            Source = "https://octopus.myget.org/F/octopus-dependencies/api/v3/index.json",
            ApiKey = EnvironmentVariable("MyGetApiKey")
        });
    }
//    if (shouldPushToNuGet)
//    {
//        NuGetPush("artifacts/Calamari." + nugetVersion + ".nupkg", new NuGetPushSettings {
//            Source = "https://www.nuget.org/api/v2/package",
//            ApiKey = EnvironmentVariable("NuGetApiKey")
//        });
//        NuGetPush("artifacts/Calamari." + nugetVersion + ".symbols.nupkg", new NuGetPushSettings {
//            Source = "https://www.nuget.org/api/v2/package",
//            ApiKey = EnvironmentVariable("NuGetApiKey")
//        });
//    }
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("__Default");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
