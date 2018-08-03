using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using static Environment;
    using static BuildResultCode;

    /// <summary>
    /// Building also requires that we ensure that the packages are fully restored. Unfortunately,
    /// however, this appears to be a step apart from the build service itself.
    /// </summary>
    /// <inheritdoc cref="IClassFixture{T}" />
    public class BumpVersionBuildIntegrationTests
        : IClassFixture<NuGetInvocationService>
            , IClassFixture<LatestBuildInvocationServiceFactory>
    {
        private ITestOutputHelper OutputHelper { get; }

        private NuGetInvocationService NuGetInvocationService { get; }

        private MSBuildInvocationService BuildInvocationService { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nuGetInvocationService"></param>
        /// <param name="buildInvocationServiceFactory"></param>
        /// <param name="outputHelper"></param>
        public BumpVersionBuildIntegrationTests(NuGetInvocationService nuGetInvocationService
            , LatestBuildInvocationServiceFactory buildInvocationServiceFactory, ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            NuGetInvocationService = nuGetInvocationService;
            BuildInvocationService = buildInvocationServiceFactory.GetService(outputHelper);
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

                NuGetInvocationService.Restore(projOrSlnFullPath);

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
                    var assy = typeof(BumpVersionBuildIntegrationTests).Assembly;
                    var assyDir = new FileInfo(assy.Location).Directory;
                    Assert.NotNull(assyDir);
                    Assert.NotNull(assyDir.Parent);
                    Assert.NotNull(assyDir.Parent.Parent);
                    Assert.NotNull(assyDir.Parent.Parent.Parent);
                    // TODO: TBD: we want the one solution? or each of the projects?
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
