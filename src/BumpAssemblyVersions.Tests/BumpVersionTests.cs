using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using static Environment;
    using static BuildResultCode;
    using static LoggerVerbosity;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc cref="IClassFixture{T}" />
    public class BumpVersionTests : IClassFixture<NuGetInvocationService>
    {
        private ITestOutputHelper OutputHelper { get; }

        private NuGetInvocationService NugetInvocationService { get; }

        private MSBuildInvocationService BuildInvocationService { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nugetInvocationService"></param>
        /// <param name="outputHelper"></param>
        public BumpVersionTests(NuGetInvocationService nugetInvocationService, ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            NugetInvocationService = nugetInvocationService;
            //const LoggerVerbosity desiredVerbosity = Diagnostic;
            const LoggerVerbosity desiredVerbosity = Normal;
            BuildInvocationService = new LatestBuildInvocationService(outputHelper, desiredVerbosity);
        }

        //// TODO: TBD: the whole mocking thing was also an attempt to work through the issue
        //// TODO: TBD: however, I really do want the build to work, and to analyze hard measurable before/after results...
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="buildProject"></param>
        ///// <param name="consoleOutput"></param>
        ///// <returns></returns>
        //public bool CallMsBuild(string buildProject, out string consoleOutput)
        //{var p = new ProjectCollection();
        //    var mockEngine = new Mock<IBuildEngine>();
        //    var task = new BumpVersionTask();
        //    task.BuildEngine = mockEngine.Object;
        //    consoleOutput = string.Empty;
        //    return task.Execute();
        //}

        //[Fact]
        //public void Test()
        //{
        //    CallMsBuild(string.Empty, out var s);
        //}

        //// TODO: TBD: Interesting, but probably unnecessary after all...
        //private string GetToolsetInstallationPath()
        //{
        //    return new SetupConfiguration()
        //        .GetInstanceForCurrentProcess().GetInstallationPath();
        //}
        //private bool IsComponentInstalled(string packageId)
        //{
        //    if (packageId == null)
        //    {
        //        throw new ArgumentNullException(nameof(packageId));
        //    }

        //    var instance = (ISetupInstance2) new SetupConfiguration().GetInstanceForCurrentProcess();
        //    return instance.GetPackages().Any(package => string.Equals(
        //        package.GetId(), packageId, StringComparison.OrdinalIgnoreCase));
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projOrSlnFullPath"></param>
        /// <param name="expectedResultCode"></param>
        [Theory, MemberData(nameof(BuildVerificationTestCases))]
        public void Verify_build_results(string projOrSlnFullPath, BuildResultCode expectedResultCode)
        {
            void ToolsetRequiredCallback(object sender, ToolsetRequiredEventArgs e)
            {
                const string expectedToolsVersion = "15.0";
                e.Predicate = ts => ts.ToolsVersion == expectedToolsVersion;
                e.GetInstallDirectoryName = ts => new FileInfo(ts.ToolsPath).Directory?.Parent?.Parent?.FullName;
            }

            void ConfigureEnvironmentVariablesCallback(object sender, ConfigureEnvironmentVariablesEventArgs e)
            {
                /* These do not seem to impact one way or another, but probably should be done just
                 for sake of consistency regardless of the MSBuild version we're running against. */
                SetEnvironmentVariable("VSINSTALLDIR", e.InstallDirectoryName);
                SetEnvironmentVariable("MSBUILD_EXE_PATH", e.Toolset.ToolsPath);
                SetEnvironmentVariable("VisualStudioVersion", e.Toolset.ToolsVersion);
            }

            void ConfigureBuildCallback(object sender, ConfigureBuildEventArgs e)
            {
                e.GlobalProperties = new Dictionary<string, string>
                {
                    {"Configuration", "Debug"},
                    {"Platform", "Any CPU"}
                };

                e.ProjectOrSolutionFullPath = projOrSlnFullPath;

                const string rebuild = "Rebuild";
                e.TargetsToBuild = new[] {rebuild};
            }

            void AfterBuildCallback(object sender, BuildResultEventArgs e)
            {
                Assert.Equal(expectedResultCode, e.Result.OverallResult);
            }
            
            try
            {
                BuildInvocationService.ToolsetRequired += ToolsetRequiredCallback;
                BuildInvocationService.ConfigureEnvironmentVariables += ConfigureEnvironmentVariablesCallback;
                BuildInvocationService.ConfigureBuild += ConfigureBuildCallback;
                BuildInvocationService.AfterBuild += AfterBuildCallback;

                NugetInvocationService.Restore(projOrSlnFullPath);

                BuildInvocationService.Run();
            }
            finally
            {
                BuildInvocationService.ToolsetRequired -= ToolsetRequiredCallback;
                BuildInvocationService.ConfigureEnvironmentVariables -= ConfigureEnvironmentVariablesCallback;
                BuildInvocationService.ConfigureBuild -= ConfigureBuildCallback;
                BuildInvocationService.AfterBuild -= AfterBuildCallback;
            }
        }

        private static IEnumerable<object[]> _buildVerificationTestCases;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> BuildVerificationTestCases
        {
            get
            {
                IEnumerable<object[]> Get()
                {
                    var assy = typeof(BumpVersionTests).Assembly;
                    var assyDir = new FileInfo(assy.Location).Directory;
                    var slnPath = assyDir.Parent.Parent.Parent
                        .GetDirectories("TestSolution").Single()
                        .GetFiles("TestSolution.sln").Single().FullName;

                    yield return new object[] {slnPath, Success};
                }

                return _buildVerificationTestCases ?? (_buildVerificationTestCases = Get());
            }
        }
    }
}
