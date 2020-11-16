using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Bav.Integration
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

        /// <summary>
        /// Verifies the Build Results given the Inputs.
        /// </summary>
        /// <param name="projOrSlnFullPath"></param>
        /// <param name="buildTargets"></param>
        /// <param name="globalProperties"></param>
        /// <param name="expectedResultCode"></param>
        /// <param name="evaluateException"></param>
        /// <param name="verifyVersions"></param>
        /// <param name="filterSources"></param>
        [
            // TODO: TBD: skipping for now... this could work after all, but running into some SDK issues, I think.
            // TODO: TBD: see notes sprinkled throughout for potential opportunities, clues, gaps, etc...
            Theory
                (Skip = "Ignoring for now, I think it is perhaps a shadow copying issue.")
            , MemberData(nameof(BuildVerificationTestCases))
        ]
        public void Verify_build_results(string projOrSlnFullPath
            , IDictionary<string, string> globalProperties, IEnumerable<string> buildTargets
            , BuildResultCode expectedResultCode, Func<Exception, bool> evaluateException
            , Action<IEnumerable<string>, IEnumerable<string>> verifyVersions, IEnumerable<IFilterSource> filterSources)
        {
            OutputHelper.WriteLine($"Attempting to build: '{projOrSlnFullPath}'");

            void OnToolsetRequired(object sender, ToolsetRequiredEventArgs e)
            {
                // TODO: TBD: could probably update to toolset 16.0 (VS2019)...
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
                // TODO: TBD: may possible have to set environment variable: MSBuildSDKsPath:
                // https://stackoverflow.com/questions/46257393/msbuild-throws-error-the-sdk-microsoft-net-sdk-specified-could-not-be-found
                // {"The SDK 'Microsoft.NET.Sdk' specified could not be found. L:\\Source\\Ellumination Technologies\\BumpAssemblyVersions\\master\\usage\\direct\\NetStd.ProjXml.AllowWildcard.FileInfoVersions\\NetStd.ProjXml.AllowWildcard.FileInfoVersions.csproj"}
            }

            void OnConfigureBuild(object sender, ConfigureBuildEventArgs e)
            {
                e.GlobalProperties = globalProperties;
                e.ProjectOrSolutionFullPath = projOrSlnFullPath;
                // ReSharper disable once PossibleMultipleEnumeration
                e.TargetsToBuild = buildTargets.ToArray();
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
                IEnumerable<object[]> Get()
                {
                    var globalProperties = new Dictionary<string, string>
                    {
                        {"Configuration", "Debug"},
                        {"Platform", "AnyCPU"}
                    };

                    IEnumerable<string> GetBuildTargets(params string[] targets) => targets;

                    var buildTargets = GetBuildTargets("Rebuild");

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
                        yield return globalProperties;
                        yield return buildTargets;
                        yield return expectedResultCode;
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (expectedResultCode)
                        {
                            case Success:
                                yield return (Func<Exception, bool>) (ex => ex == null);
                                break;
                            case Failure:
                                yield return (Func<Exception, bool>) (ex => ex != null);
                                break;
                        }

                        yield return verifyVersions;

                        // Return FilterSources as an Enumerable.
                        var fs = (IEnumerable<IFilterSource>) filterSources;
                        yield return fs;
                    }

                    // Renamed for better clarity, these three are intended as strategic callbacks.
                    void OnAllDifferent(IEnumerable<string> before, IEnumerable<string> after)
                        => Assert.All(before.Zip(after, (b, a) => b != a), Assert.True);

                    void OnAllEqual(IEnumerable<string> before, IEnumerable<string> after)
                        => Assert.All(before.Zip(after, (b, a) => b == a), Assert.True);

                    void OnNoneGiven(IEnumerable<string> before, IEnumerable<string> after)
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
                    Assert.NotNull(assyDir.Parent.Parent.Parent.Parent);
                    Assert.NotNull(assyDir.Parent.Parent.Parent.Parent.Parent);
                    // Remember the projects did refactor location in the overall repository, which this addresses.
                    var slnDir = assyDir.Parent.Parent.Parent.Parent.Parent.GetDirectories(@"usage\direct").Single();

                    string GetProjectFullPath(string fileName)
                        => slnDir.GetFiles($"{fileName}.csproj", AllDirectories).Single().FullName;

                    // TODO: TBD: I believe this gets me a bit closer to what I wanted to accomplish in these scenarios...
                    // TODO: TBD: next is to evaluate the filters before compilation and after compilation
                    // TODO: TBD: and which ever bits are being screened should see a marked change
                    // TODO: TBD: after that, may see about a NuGetVersion friendly result
                    // TODO: TBD: which I think should support not only Version but also the <version/>-<prereleaselabel/>
                    yield return GetOne(GetProjectFullPath("NetStd.ProjXml.AllowWildcard.FileInfoVersions"), Success, OnAllDifferent
                        , GetProjectXmlVersionFilterSource(GetProjectFullPath("NetStd.ProjXml.AllowWildcard.FileInfoVersions")
                            , new ProjectXmlVersionFilter {Kind = AssemblyVersion}
                            , new ProjectXmlVersionFilter {Kind = FileVersion}
                            , new ProjectXmlVersionFilter {Kind = InformationalVersion}
                        )
                    ).ToArray();

                    yield return GetOne(GetProjectFullPath("NetStd.ProjXml.TestLookupNoDups"), Success, OnAllDifferent
                        , GetProjectXmlVersionFilterSource(GetProjectFullPath("NetStd.ProjXml.TestLookupNoDups")
                            , new ProjectXmlVersionFilter {Kind = VersionKind.Version}
                            , new ProjectXmlVersionFilter {Kind = AssemblyVersion}
                            , new ProjectXmlVersionFilter {Kind = FileVersion}
                            , new ProjectXmlVersionFilter {Kind = InformationalVersion}
                            , new ProjectXmlVersionFilter {Kind = PackageVersion}
                        )
                    ).ToArray();

                    yield return GetOne(GetProjectFullPath("AssyInfo.FileInfoVersions.IncludeWildcards"), Success, OnAllDifferent
                        , GetAssemblyAttributeVersionFilterSource(
                            GetProjectFullPath("AssyInfo.FileInfoVersions.IncludeWildcards")
                            , Combine("Properties", "AssemblyInfo.cs")
                            , new ShortAssemblyAttributeVersionFilter<AssemblyVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyFileVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyInformationalVersionAttribute>()
                        )
                    ).ToArray();

                    yield return GetOne(GetProjectFullPath("AssyAttributes.NoBumpSpecs"), Success, OnAllEqual
                        , GetAssemblyAttributeVersionFilterSource(
                            GetProjectFullPath("AssyAttributes.NoBumpSpecs")
                            , Combine("Properties", "AssemblyInfo.cs")
                            , new ShortAssemblyAttributeVersionFilter<AssemblyVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyFileVersionAttribute>()
                            , new ShortAssemblyAttributeVersionFilter<AssemblyInformationalVersionAttribute>()
                        )
                    ).ToArray();

                    // TODO: TBD: this bit is not testing anything apart from the test theory plumbing itself...
                    // TODO: TBD: nothing to compare here, or we might even expect them all to be empty...
                    yield return GetOne(GetProjectFullPath("Should.Reject.Invalid.Specs"), Failure, OnNoneGiven).ToArray();

                    // TODO: TBD: ...we also need to ensure that we include the winforms, wpf, etc, projects.
                }

                return _buildVerificationTestCases ?? (_buildVerificationTestCases = Get());
            }
        }
    }
}
