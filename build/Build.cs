using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    readonly string AspNetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

    AbsolutePath OutputDirectory => RootDirectory / "Output";
    AbsolutePath ClientDirectory => RootDirectory / "Client";
    AbsolutePath ServerDirectory => RootDirectory / "API"; 
    AbsolutePath WwwRoot => ServerDirectory / "wwwroot";
    AbsolutePath ProjectFile => ServerDirectory / "DotNetAngularTemplate.csproj";

    Target Clean => _ => _
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
            WwwRoot.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(ProjectFile));
        });

    Target BuildAngular => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            ProcessTasks.StartProcess("npm", "ci", ClientDirectory).AssertZeroExitCode();
            ProcessTasks.StartProcess("npm", "run build", ClientDirectory).AssertZeroExitCode();
        });

    Target CompileDotNet => _ => _
        .DependsOn(BuildAngular)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(ProjectFile)
                .SetConfiguration(Configuration));
        });

    Target Publish => _ => _
        .DependsOn(CompileDotNet)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(ProjectFile)
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory));
        });
}
