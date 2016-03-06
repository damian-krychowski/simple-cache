var target = Argument("target", "Default");

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore("../Source/SimpleCache.sln");
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories("../Source/**/bin");
    CleanDirectories("../Source/**/obj");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    MSBuild("../Source/SimpleCache.sln", settings => settings.SetConfiguration("Release"));
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("../Source/SimpleCache.Tests/bin/Release/SimpleCache.Tests.dll", new NUnitSettings {
        ToolPath = "../Source/packages/NUnit.ConsoleRunner.3.2.0/tools/nunit3-console.exe"
    });
	
	NUnit("../Source/SimpleCache.ConcurrencyTests/bin/Release/SimpleCache.ConcurrencyTests.dll", new NUnitSettings {
        ToolPath = "../Source/packages/NUnit.ConsoleRunner.3.2.0/tools/nunit3-console.exe"
    });
});

Task("Default")
	.IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    Information("SimpleCache building finished.");
});

RunTarget(target);