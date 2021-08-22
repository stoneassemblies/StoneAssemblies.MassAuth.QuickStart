var target = Argument("target", "NuGetPack");
var buildConfiguration = Argument("Configuration", "Release");

var NuGetVersionV2 = "1.0.0";
var SolutionFileName = "src/StoneAssemblies.MassAuth.QuickStart.sln";
var ComponentProjects  = new string[] 
{
	"./src/StoneAssemblies.MassAuth.QuickStart.Messages/StoneAssemblies.MassAuth.QuickStart.Messages.csproj",
	"./src/StoneAssemblies.MassAuth.QuickStart.Rules/StoneAssemblies.MassAuth.QuickStart.Rules.csproj"
};

Task("Restore")
  .Does(() => 
    {
        DotNetCoreRestore(SolutionFileName);
    }); 

Task("Build")
  .IsDependentOn("Restore")
  .Does(() => 
    {
        DotNetCoreBuild(
                    SolutionFileName,
                    new DotNetCoreBuildSettings()
                    {
                        Configuration = buildConfiguration,
                        ArgumentCustomization = args => args
                            .Append($"/p:Version={NuGetVersionV2}")
                            .Append($"/p:PackageVersion={NuGetVersionV2}")
                    });
    }); 

Task("NuGetPack")
  .IsDependentOn("Build")
  .Does(() => 
    {
        string packageOutputDirectory = $"./output/nuget";

        EnsureDirectoryExists(packageOutputDirectory);
        CleanDirectory(packageOutputDirectory);

        for (int i = 0; i < ComponentProjects.Length; i++)
        {
            var componentProject = ComponentProjects[i];
            var settings = new DotNetCorePackSettings
            {
                Configuration = buildConfiguration,
                OutputDirectory = packageOutputDirectory,
                IncludeSymbols = true,
                ArgumentCustomization = args => args
                    .Append($"/p:PackageVersion={NuGetVersionV2}")
                    .Append($"/p:Version={NuGetVersionV2}")
            };

            DotNetCorePack(componentProject, settings);
        }

        EnsureDirectoryExists("./output/nuget-symbols");
        CleanDirectory("./output/nuget-symbols");

        MoveFiles($"{packageOutputDirectory}/*.symbols.nupkg", "./output/nuget-symbols");
        var symbolFiles  = GetFiles("./output/nuget-symbols/*.symbols.nupkg");
        foreach(var symbolFile in symbolFiles)
        {
            var newFileName = symbolFile.ToString().Replace(".symbols", "");
            MoveFile(symbolFile, newFileName);
        }
    });


RunTarget(target);