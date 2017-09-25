#tool "nuget:?package=xunit.runner.console"
#addin "Cake.Json"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var uri = Argument("uri", "https://localhost:8081/");
var authKey = Argument("authKey", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
var consistencyLevel = Argument("consistencyLevel", "BoundedStaleness");
var buildVersion = Argument("buildVersion", "1.0.0");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var solutionDir = "../src/";
var solutionFile = solutionDir + "SimpleEventStore.sln";
var documentDbTestConfigFiles = new [] {
	File("../src/SimpleEventStore.AzureDocumentDb.Tests/bin/" + configuration + "/netcoreapp1.1/appsettings.json"),
	File("../src/SimpleEventStore.AzureDocumentDb.Tests/bin/" + configuration + "/net452/appsettings.json")
};
var testDirs = GetDirectories(solutionDir + "*.Tests");
var outputDir = "./nuget";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Restore-Packages")
    .Does(() =>
{
    DotNetCoreRestore(solutionDir);
});

Task("Clean")
    .Does(() => 
{
    CleanDirectory(outputDir);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
{
    DotNetCoreBuild(solutionFile, new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoIncremental = true,
        ArgumentCustomization = args => args.Append("/p:BuildVersion=" + buildVersion)
    });
});

Task("Transform-Unit-Test-Config")
    .Does(() =>
{
	foreach(var documentDbTestConfigFile in documentDbTestConfigFiles)
	{
		var configJson = ParseJsonFromFile(documentDbTestConfigFile);
		configJson["Uri"] = uri;
		configJson["AuthKey"] = authKey;
		configJson["ConsistencyLevel"] = consistencyLevel;

		SerializeJsonToFile(documentDbTestConfigFile, configJson);
	}
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .IsDependentOn("Transform-Unit-Test-Config")
    .Does(() =>
{
    foreach(var testPath in testDirs)
    {
        var returnCode = StartProcess("dotnet", new ProcessSettings {
            Arguments = "xunit -c " + configuration + " --no-build",
            WorkingDirectory = testPath
        });

        if(returnCode != 0)
        {
            throw new Exception("Unit test failure for test project " + testPath);
        }
    }
});

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    var settings = new DotNetCorePackSettings {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = outputDir,
        ArgumentCustomization = args => args.Append("/p:BuildVersion=" + buildVersion)
    };

    DotNetCorePack("./../src/SimpleEventStore/", settings);
    DotNetCorePack("./../src/SimpleEventStore.AzureDocumentDb/", settings);
});


Task("Deploy")
    .IsDependentOn("Package")
    .Does(() => 
{
    var nugetSource = Argument<string>("nugetSource");
    var nugetApiKey = Argument<string>("nugetApiKey");

    var package = GetFiles(outputDir + "/*.nupkg");

    NuGetPush(package, new NuGetPushSettings {
        Source = nugetSource,
        ApiKey = nugetApiKey
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
