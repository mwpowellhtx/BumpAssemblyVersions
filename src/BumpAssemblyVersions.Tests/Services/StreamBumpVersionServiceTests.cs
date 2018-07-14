using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using static RegexOptions;
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StreamBumpVersionServiceTests<T>
        where T : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        protected StreamBumpVersionServiceTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        protected static readonly Type FixturedType = typeof(T);

        private class VersionProviderSamenessComparer : EqualityComparer<IVersionProvider>
        {
            public override bool Equals(IVersionProvider x, IVersionProvider y)
                => !(x == null || y == null) && ReferenceEquals(x, y);

            public override int GetHashCode(IVersionProvider obj)
                => (obj?.Id ?? Guid.Empty).GetHashCode();
        }

        private static IEnumerable<Action<Regex>> GetExpectedRegexVerification()
        {
            bool ShortNameRegexExpected() => FixturedType.Name.EndsWith(nameof(Attribute));

            if (ShortNameRegexExpected())
            {
                yield return regex =>
                {
                    Assert.NotNull(regex);
                    Assert.Equal(Compiled, regex.Options);
                    Assert.Contains($"{FixturedType.ToShortName()}", $"{regex}");
                };
            }

            yield return regex =>
            {
                Assert.NotNull(regex);
                Assert.Equal(Compiled, regex.Options);
                Assert.Contains($"{FixturedType.ToLongName()}", $"{regex}");
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="versionProviders"></param>
        /// <returns></returns>
        internal static IStreamBumpVersionService CreateFixture(
            ServiceMode mode, params IVersionProvider[] versionProviders)
        {
            var serviceFixture = new StreamBumpVersionServiceFixture<T>(versionProviders) {Mode = mode};

            VerifyFixture(serviceFixture, mode, versionProviders);

            // We should also be able to identify definitive Regular Expressions characteristics.
            Assert.Collection(serviceFixture.AttributeRegexes, GetExpectedRegexVerification().ToArray());

            return serviceFixture;
        }

        private static void VerifyFixture(IBumpVersionService service, ServiceMode expectedMode
            , params IVersionProvider[] expectedProviders)
        {
            Assert.NotNull(service);

            Assert.Equal(expectedMode, service.Mode);

            // Does not need to be the same precise instance, but each of the items should be.
            Assert.Equal(expectedProviders.Length, service.VersionProviders.Count());

            if (expectedProviders.Any())
            {
                Assert.Equal(expectedProviders, service.VersionProviders, new VersionProviderSamenessComparer());
            }
        }

        private static IVersionProvider NoOp => new NoOpVersionProvider();

        private static IVersionProvider Unknown => new UnknownVersionProvider();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="given"></param>
        /// <param name="expectedVersion"></param>
        /// <param name="shouldMatch"></param>
        [Theory]
        [MemberData(nameof(DefaultTestCases))]
        public void Verify_Default_Fixture(ServiceMode mode, string given, string expectedVersion, bool shouldMatch)
        {
            var expectedProviders = new[] {NoOp, NoOp, NoOp, NoOp};

            var fixture = CreateFixture(mode, expectedProviders);

            Assert.Equal(shouldMatch, fixture.AttributeRegexes
                .FilterMatch(given, shouldMatch)
                .TryVerifyMatch(given, expectedVersion, shouldMatch));
        }

        private static IEnumerable<object[]> _defaultTestCases;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> DefaultTestCases
        {
            get
            {
                // The couple of sort of boilerplate usages are captured here.
                string GetShortNameUsage(string version)
                    => $"[assembly: {FixturedType.ToShortName()}(\"{version}\")]";

                string GetLongNameUsage(string version)
                    => $"[assembly: {FixturedType.ToLongName()}(\"{version}\")]";

                IEnumerable<object> GetOne(ServiceMode mode, string g, string s, bool shouldMatch)
                {
                    yield return mode;
                    yield return g;
                    yield return s;
                    yield return shouldMatch && g.Contains(s);
                }

                IEnumerable<object[]> GetAll()
                {
                    // And we may further contain the set of Matches according to whether ShouldMatch.
                    IEnumerable<object[]> GetKnownMatches(bool shouldMatch, params string[] versions)
                    {
                        IEnumerable<ServiceMode> GetServiceModes()
                        {
                            yield return VersionElements;
                            yield return ReleaseElements;
                            yield return VersionAndReleaseElements;
                            yield return MetadataElements;
                        }

                        foreach (var mode in GetServiceModes().ToArray())
                        {
                            foreach (var version in versions)
                            {
                                yield return GetOne(mode, GetShortNameUsage(version), version, shouldMatch).ToArray();
                                yield return GetOne(mode, GetLongNameUsage(version), version, shouldMatch).ToArray();
                            }
                        }
                    }

                    // TODO: TBD: this will need to improve a bit: currently demonstrating the sort of naive pattern matching working...
                    // Which allows us to focus on the Version themselves here.
                    foreach (var testCase in GetKnownMatches(true
                        , "..."
                        , ".-+"
                        , "1234567890.abcdefghijklmnopqrstuvwxyz..ABCDEFGHIJKLMNOPQRSTUVWXYZ-+*"
                    ))
                    {
                        yield return testCase;
                    }
                }

                return _defaultTestCases ?? (_defaultTestCases = GetAll().ToArray());
            }
        }
    }
}
