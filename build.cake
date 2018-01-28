var target = Argument("target", "Default");
var config = Argument("config", "Release");
var output = Argument("output", "./output");

var outputDir = Directory(output);
var sln = File("./nez.sln");
var projects = new Lazy<SolutionParserResult>(() => ParseSolution(sln))
	.Value
	.Projects
	.Where(p => FileExists(p.Path.ChangeExtension(".nuspec")))
	.Select(p => p.Path);

Task("Clean")
	.Does(() =>
	{
		CleanDirectory(outputDir);

		DotNetBuild(sln, settings => settings
			.SetConfiguration(config)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));
	});

Task("Restore")
	.IsDependentOn("Clean")
	.Does(() => NuGetRestore(sln));

Task("Build")
	.IsDependentOn("Restore")
	.Does(() => 
	{ 
		DotNetBuild(sln, settings => settings
			.SetConfiguration(config)
			.WithTarget("Rebuild"));
	});

Task("Package")
	.IsDependentOn("Build")
	.DoesForEach(projects, (project) =>
	{
		NuGetPack(project, new NuGetPackSettings
		{
			OutputDirectory = outputDir,
			Properties = new Dictionary<string, string>
			{
				{ "Configuration", config }
			}
		});
	});

Task("Default")
	.IsDependentOn("Package");

RunTarget(target);