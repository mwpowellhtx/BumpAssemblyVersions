using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using static Environment;
    using static Path;
    using static String;
    using static BuildResultCode;
    using static LoggerVerbosity;
    using static SearchOption;
    using static VersionKind;

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
        /// <param name="verifyVersions"></param>
        /// <param name="filterSources"></param>
        [Theory, MemberData(nameof(BuildVerificationTestCases))]
        public void Verify_build_results(string projOrSlnFullPath, BuildResultCode expectedResultCode
            , Func<InvalidOperationException, bool> evaluateException
            , Action<IEnumerable<string>, IEnumerable<string>> verifyVersions
            , IEnumerable<IFilterSource> filterSources)
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
                 * for sake of consistency regardless of the MSBuild version we're running against. */
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

            void ReportVersions(string message, params string[] versions)
            {
                if (!versions.Any())
                {
                    OutputHelper.WriteLine("There are no versions reported.");
                    return;
                }

                OutputHelper.WriteLine($"{message}: {Join(", ", versions.Select(v => $"'{v}'"))}");
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

                // ReSharper disable once PossibleMultipleEnumeration
                var versionsBefore = filterSources.SelectMany(source => source.Versions).ToArray();

                ReportVersions("In no particular order, the version(s) of interest before are", versionsBefore);

                nis.Restore(projOrSlnFullPath);

                bis.Run();

                // ReSharper disable once PossibleMultipleEnumeration
                var versionsAfter = filterSources.SelectMany(source => source.Versions).ToArray();

                ReportVersions("In no particular order, the version(s) of interest after are", versionsAfter);

                verifyVersions?.Invoke(versionsBefore, versionsAfter);
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
                bool SuccessExceptionEvaluation(InvalidOperationException ex) => ex == null;
                bool FailureExceptionEvaluation(InvalidOperationException ex) => ex != null;

                IEnumerable<object[]> Get()
                {
                    IFilterSource GetAssemblyAttributeVersionFilterSource(
                        string projectFullPath, string relativePath
                        , params IVersionFilter[] filters)
                        => new AssemblyAttributeVersionFilterSource(filters)
                        {
                            ProjectFullPath = projectFullPath,
                            RelativePath = relativePath
                        };

                    IFilterSource GetProjectXmlVersionFilterSource(string projectFullPath
                        , params IVersionFilter[] filters)
                        => new ProjectXmlVersionFilterSource(filters) {ProjectFullPath = projectFullPath};

                    IEnumerable<object> GetOne(string csprojFullName, BuildResultCode expectedResultCode
                        , Action<IEnumerable<string>, IEnumerable<string>> verifyVersions
                        , params IFilterSource[] filterSources)
                    {
                        yield return csprojFullName;
                        // TODO: TBD: work out the particulars here; make sure that it aligns with the test method, etc...
                        yield return expectedResultCode;
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (expectedResultCode)
                        {
                            case Success:
                                yield return (Func<InvalidOperationException, bool>) SuccessExceptionEvaluation;
                                break;
                            case Failure:
                                yield return (Func<InvalidOperationException, bool>) FailureExceptionEvaluation;
                                break;
                        }

                        yield return verifyVersions;

                        // Return FilterSources as an Enumerable.
                        var fs = (IEnumerable<IFilterSource>) filterSources;
                        yield return fs;
                    }

                    void AllDifferent(IEnumerable<string> before, IEnumerable<string> after)
                        => Assert.All(before.Zip(after, (b, a) => b != a), Assert.True);

                    void AllEqual(IEnumerable<string> before, IEnumerable<string> after)
                        => Assert.All(before.Zip(after, (b, a) => b == a), Assert.True);

                    void NoneGiven(IEnumerable<string> before, IEnumerable<string> after)
                        => Assert.False(before.Any() || after.Any());

                    /* Does not seem to be working when I isolate a Project for build.
                     * See issue: BuildManager.Build unable to build a CSPROJ path /
                     * http://github.com/Microsoft/msbuild/issues/3636
                     * It was necessary to update not only Visual Studio, but also the
                     * packages which I referenced in order for it to work properly.
                     * TODO: TBD: http://docs.microsoft.com/en-us/visualstudio/msbuild/updating-an-existing-application */
                    var assy = typeof(BumpVersionBuildIntegrationTests).Assembly;
                    var assyDir = new FileInfo(assy.Location).Directory;
                    Assert.NotNull(assyDir);
                    Assert.NotNull(assyDir.Parent);
                    Assert.NotNull(assyDir.Parent.Parent);
                    Assert.NotNull(assyDir.Parent.Parent.Parent);
                    // TODO: TBD: we want the one solution? or each of the projects?
                    var slnDir = assyDir.Parent.Parent.Parent
                        .GetDirectories("TestSolution").Single();
                    // TODO: TBD: may not be the best possible factoring for these methods?
                    string GetProjectFullPath(string fileName, string extension = ".csproj")
                        => slnDir.GetFiles($"{fileName}{extension}", AllDirectories).Single().FullName;

                    //// TODO: TBD: get away from "get all" for this part; we do need to contain each test case, including filter sources and version filters...
                    //IEnumerable<object[]> GetAll(BuildResultCode expectedResultCode
                    //    , Func<InvalidOperationException, bool> exceptionEvaluation
                    //    , params string[] csprojFileNames)
                    //{
                    //    foreach (var csprojFileName in csprojFileNames)
                    //    {
                    //        yield return GetOne(slnDir.GetFiles($"{csprojFileName}.csproj", AllDirectories)
                    //            .Single().FullName, exceptionEvaluation, expectedResultCode).ToArray();
                    //    }
                    //}

                    // TODO: TBD: I believe this gets me a bit closer to what I wanted to accomplish in these scenarios...
                    // TODO: TBD: next is to evaluate the filters before compilation and after compilation
                    // TODO: TBD: and which ever bits are being screened should see a marked change
                    // TODO: TBD: after that, may see about a NuGetVersion friendly result
                    // TODO: TBD: which I think should support not only Version but also the <version/>-<prereleaselabel/>
                    yield return GetOne(GetProjectFullPath("AssyVersion_NetStandard"), Success, AllDifferent
                        , GetProjectXmlVersionFilterSource(GetProjectFullPath("AssyVersion_NetStandard")
                            , new ProjectXmlVersionFilter {Kind = AssemblyVersion}
                            , new ProjectXmlVersionFilter {Kind = FileVersion}
                            , new ProjectXmlVersionFilter {Kind = InformationalVersion}
                        )
                    ).ToArray();

                    yield return GetOne(GetProjectFullPath("AssyVersion_PatchWildcard"), Success, AllDifferent
                        , GetAssemblyAttributeVersionFilterSource(
                            GetProjectFullPath("AssyVersion_PatchWildcard")
                            , Combine("Properties", "AssemblyInfo.cs")
                            , new ShortAssemblyAttributeVersionFilter<AssemblyVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyFileVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyInformationalVersionAttribute>()
                        )
                    ).ToArray();

                    yield return GetOne(GetProjectFullPath("Proj.NF.AssyVersion"), Success, AllEqual
                        , GetAssemblyAttributeVersionFilterSource(
                            GetProjectFullPath("Proj.NF.AssyVersion")
                            , Combine("Properties", "AssemblyInfo.cs")
                            , new ShortAssemblyAttributeVersionFilter<AssemblyVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyFileVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyInformationalVersionAttribute>()
                        )
                    ).ToArray();

                    // TODO: TBD: this bit is not testing anything apart from the test theory plumbing itself...
                    // TODO: TBD: nothing to compare here, or we might even expect them all to be empty...
                    yield return GetOne(GetProjectFullPath("ProjectA"), Failure, NoneGiven).ToArray();
                }

                return _buildVerificationTestCases ?? (_buildVerificationTestCases = Get());
            }
        }
    }
}
