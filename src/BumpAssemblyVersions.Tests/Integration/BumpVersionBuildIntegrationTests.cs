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
    using static SearchOption;

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

            BuildInvocationService.ConfigureBuild += (sender, e) =>
            {
                e.Loggers.ToList().ForEach(
                    logger => logger.Verbosity = Detailed
                );
            };
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
        /// <param name="evaluateException"></param>
        [Theory, MemberData(nameof(BuildVerificationTestCases))]
        public void Verify_build_results(string projOrSlnFullPath, BuildResultCode expectedResultCode
            , Func<InvalidOperationException, bool> evaluateException)
        {
            OutputHelper.WriteLine($"Attempting to build: '{projOrSlnFullPath}'");

            // TODO: TBD: I want to specify the project file itself, not the solution file, I think...
            // TODO: TBD: BuildResultCode is probably still appropriate here
            // TODO: TBD: I want to test for not only build result, but did the expected versions numbers actually bump
            // TODO: TBD: somehow open the project, glimpse its Items for their actual paths
            // TODO: TBD: find the Xml and/or Assembly Attribute based versions
            // TODO: TBD: compare the before with the after/expected values
            // TODO: TBD: requires some planning foreknowledge of the solution/projects under test
            // TODO: TBD: which fortunately we have, but could we better capture those?
            // TODO: TBD: a couple of things to check: first, given the project path
            // TODO: TBD: given some getters for before and after verification
            // TODO: TBD: which getters should be able to open the specific files from the test cases
            // TODO: TBD: opening the file in the subdirectory from the project file
            // TODO: TBD: could probably just expose the bits with simple regex patterns
            // TODO: TBD: although it might be nice to also validate that the Xml is valid after the project based is complete
            void OnToolsetRequired(object sender, ToolsetRequiredEventArgs e)
            {
                const string expectedToolsVersion = "15.0";
                e.Predicate = ts => ts.ToolsVersion == expectedToolsVersion;
                e.GetInstallDirectoryName = ts => new FileInfo(ts.ToolsPath).Directory?.Parent?.Parent?.FullName;
            }

            void OnConfigureEnvironmentVariables(object sender, ConfigureEnvironmentVariablesEventArgs e)
            {
                /* These do not seem to impact one way or another, but probably should be done just
                 for sake of consistency regardless of the MSBuild version we're running against. */
                SetEnvironmentVariable("VSINSTALLDIR", e.InstallDirectoryName);
                SetEnvironmentVariable("MSBUILD_EXE_PATH", e.Toolset.ToolsPath);
                SetEnvironmentVariable("VisualStudioVersion", e.Toolset.ToolsVersion);
            }

            void OnConfigureBuild(object sender, ConfigureBuildEventArgs e)
            {
                // TODO: TBD: things like Configuration, Platform, etc, may be injected...
                e.GlobalProperties = new Dictionary<string, string>
                {
                    {"Configuration", "Debug"},
                    {"Platform", "AnyCPU"}
                };

                e.ProjectOrSolutionFullPath = projOrSlnFullPath;

                // TODO: TBD: additionally, things like TargetsToBuild may also be injected...
                const string rebuild = "Rebuild";
                e.TargetsToBuild = new[] {rebuild};
            }

            void OnAfterBuild(object sender, BuildResultEventArgs e)
            {
                Assert.NotNull(e.Result);
                Assert.Null(e.Exception);
                Assert.Equal(expectedResultCode, e.Result.OverallResult);
            }

            void OnBuildExceptionOccurred(object sender, BuildResultEventArgs e)
            {
                Assert.Null(e.Result);
                Assert.NotNull(e.Exception);
                Assert.True(evaluateException(e.Exception));
            }

            var bis = BuildInvocationService;
            var nis = NuGetInvocationService;

            try
            {
                bis.ToolsetRequired += OnToolsetRequired;
                bis.ConfigureEnvironmentVariables += OnConfigureEnvironmentVariables;
                bis.ConfigureBuild += OnConfigureBuild;
                bis.AfterBuild += OnAfterBuild;
                bis.BuildExceptionOccurred += OnBuildExceptionOccurred;

                nis.Restore(projOrSlnFullPath);

                bis.Run();
            }
            finally
            {
                bis.ToolsetRequired -= OnToolsetRequired;
                bis.ConfigureEnvironmentVariables -= OnConfigureEnvironmentVariables;
                bis.ConfigureBuild -= OnConfigureBuild;
                bis.AfterBuild -= OnAfterBuild;
                bis.BuildExceptionOccurred -= OnBuildExceptionOccurred;
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
                bool SuccessExceptionEvaluation(InvalidOperationException ex) => true;

                bool FailureExceptionEvaluation(InvalidOperationException ex)
                {
                    // We would expect Failure result, but we would encounter an Exception first.
                    Assert.NotNull(ex);
                    return true;
                }

                IEnumerable<object[]> Get()
                {
                    IEnumerable<object> GetOne(string csprojFullName
                        , Func<InvalidOperationException, bool> exceptionEvaluation
                        , BuildResultCode expectedResultCode = Success)
                    {
                        yield return csprojFullName;
                        yield return expectedResultCode;
                        yield return exceptionEvaluation;
                    }

                    /* Does not seem to be working when I isolate a Project for build.
                     * See issue: BuildManager.Build unable to build a CSPROJ path /
                     * http://github.com/Microsoft/msbuild/issues/3636
                     *
                     */
                    var assy = typeof(BumpVersionBuildIntegrationTests).Assembly;
                    var assyDir = new FileInfo(assy.Location).Directory;
                    Assert.NotNull(assyDir);
                    Assert.NotNull(assyDir.Parent);
                    Assert.NotNull(assyDir.Parent.Parent);
                    Assert.NotNull(assyDir.Parent.Parent.Parent);
                    // TODO: TBD: we want the one solution? or each of the projects?
                    var slnDir = assyDir.Parent.Parent.Parent
                        .GetDirectories("TestSolution").Single();

                    IEnumerable<object[]> GetAll(BuildResultCode expectedResultCode
                        , Func<InvalidOperationException, bool> exceptionEvaluation
                        , params string[] csprojFileNames)
                    {
                        foreach (var csprojFileName in csprojFileNames)
                        {
                            yield return GetOne(slnDir.GetFiles($"{csprojFileName}.csproj", AllDirectories)
                                .Single().FullName, exceptionEvaluation, expectedResultCode).ToArray();
                        }
                    }

                    return GetAll(Success, SuccessExceptionEvaluation, "AssyVersion_NetStandard"
                            , "AssyVersion_PatchWildcard", "Proj.NF.AssyVersion")
                        .Concat(GetAll(Failure, FailureExceptionEvaluation, "ProjectA")).ToArray();
                }

                return _buildVerificationTestCases ?? (_buildVerificationTestCases = Get());
            }
        }
    }
}
