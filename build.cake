#tool nuget:?package=GitVersion.CommandLine
#tool nuget:?package=vswhere
#addin nuget:?package=Cake.Incubator

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactsDirectory = new DirectoryPath("./artifacts");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => 
{
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");
    CleanDirectories(artifactsDirectory.FullPath);

    EnsureDirectoryExists(artifactsDirectory);
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
    versionInfo = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.Json
    });

    Information("GitVersion -> {0}", versionInfo.Dump());
});

Task("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Version")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    AppVeyor.UpdateBuildVersion(AppVeyor.Environment.JobId +
        versionInfo.InformationalVersion);
});

FilePath msBuildPath;
Task("ResolveBuildTools")
    .Does(() => 
{
    var vsLatest = VSWhereLatest();
    msBuildPath = (vsLatest == null)
        ? null
        : vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");
});

Task("Build")
    .IsDependentOn("ResolveBuildTools")
    .IsDependentOn("Clean")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .Does(() => 
{
    var settings = new MSBuildSettings 
    {
        Configuration = configuration,
        ToolPath = msBuildPath,
        Verbosity = Verbosity.Minimal,
        ArgumentCustomization = args => args.Append("/m")
    };

    settings = settings.WithProperty("DebugSymbols", "True").
        WithProperty("DebugType", "Full");

    MSBuild("./src/RadialMenu/RadialMenu.sln", settings);
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() => 
{
    var nugetContent = new List<NuSpecContent>();

    var binDir = "./src/RadialMenu/RadialMenu/bin/iPhone/" + configuration;
    var files = GetFiles(binDir + "/*.dll") + GetFiles(binDir + "/*.pdb");
    foreach(var dll in files)
    {
        Information("File: {0}", dll.ToString());
        nugetContent.Add(new NuSpecContent
        {
            Target = "lib/Xamarin.iOS10",
            Source = dll.ToString()
        });
    }

    var nugetSettings = new NuGetPackSettings {
        Id = "ALRadialMenu",
        Title = "RadialMenu for Xamarin.iOS",
        Authors = new [] { "Tomasz Cielecki" },
        Owners = new [] { "Tomasz Cielecki" },
        IconUrl = new Uri("http://i.imgur.com/V3983YY.png"),
        ProjectUrl = new Uri("https://github.com/Cheesebaron/ALRadialMenu"),
        LicenseUrl = new Uri("https://raw.githubusercontent.com/Cheesebaron/ALRadialMenu/master/LICENSE"),
        Copyright = "Copyright (c) Tomasz Cielecki",
        Tags = new [] { "menu", "radial", "ios", "xamarin", "alradialmenu", "radialmenu" },
        Description = "A Radial Menu for Xamarin.iOS.",
        RequireLicenseAcceptance = false,
        Version = versionInfo.NuGetVersion,
        Symbols = false,
        NoPackageAnalysis = true,
        OutputDirectory = artifactsDirectory,
        Verbosity = NuGetVerbosity.Detailed,
        Files = nugetContent,
        BasePath = "./"
    };

    NuGetPack(nugetSettings);
});

Task("UploadAppVeyorArtifact")
    .IsDependentOn("Package")
    .WithCriteria(() => !AppVeyor.Environment.PullRequest.IsPullRequest)
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {

    Information("Artifacts Dir: {0}", artifactsDirectory.FullPath);

    var uploadSettings = new AppVeyorUploadArtifactsSettings {
        ArtifactType = AppVeyorUploadArtifactType.NuGetPackage
    };

    foreach(var file in GetFiles(artifactsDirectory.FullPath + "/*.nupkg"))
    {
        Information("Uploading {0}", file.FullPath);

        AppVeyor.UploadArtifact(file.FullPath, uploadSettings);
    }
});

Task("Default")
    .IsDependentOn("UploadAppVeyorArtifact");

RunTarget(target);