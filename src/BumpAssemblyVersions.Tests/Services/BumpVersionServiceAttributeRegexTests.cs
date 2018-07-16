using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using static BindingFlags;
    using static RegexOptions;
    using static ServiceMode;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BumpVersionServiceAttributeRegexTests<T>
        where T : Attribute
    {
        /// <summary>
        /// Provides access to an <see cref="ITestOutputHelper"/>.
        /// </summary>
        protected ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        protected BumpVersionServiceAttributeRegexTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Provides the FixtureAttributeType.
        /// </summary>
        protected static readonly Type FixtureAttributeType = typeof(T);

        /// <summary>
        /// Provides the FixtureType.
        /// </summary>
        protected static readonly Type FixtureType = typeof(StreamBumpVersionServiceFixture<T>);

        /// <summary>
        /// Verify that the <see cref="StreamBumpVersionServiceBase{T}.SupportedAttributeTypes"/>
        /// themselves are accurate and accounted for among the
        /// <paramref name="expectedSupportedTypes"/>.
        /// </summary>
        /// <param name="expectedSupportedTypes"></param>
        [Theory, MemberData(nameof(ExpectedSupportedTypes))]
        public void Verify_Supported_Attribute_Type(params Type[] expectedSupportedTypes)
        {
            const string propertyName = nameof(StreamBumpVersionServiceBase<T>.SupportedAttributeTypes);

            // Not technically from the FixtureType, per se.
            var property = typeof(StreamBumpVersionServiceBase<T>).GetProperty(propertyName, Static | NonPublic | GetProperty);

            Assert.NotNull(property);
            Assert.Equal(typeof(IEnumerable<Type>), property.PropertyType);

            // While technically we could just Get the Value, let's also assert that we did.
            bool TryGetPropertyValue(PropertyInfo pi, out IEnumerable<Type> types)
            {
                types = (IEnumerable<Type>) pi.GetValue(null);
                return types != null && types.Any();
            }

            // In other words, we are not expecting any Exceptions be thrown from the Static Ctor.
            Assert.True(TryGetPropertyValue(property, out var actualSupportedTypes));

            // ReSharper disable once PossibleMultipleEnumeration
            Assert.Equal(expectedSupportedTypes.Length, actualSupportedTypes.Count());

            foreach (var expectedSupportedType in expectedSupportedTypes)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                Assert.Contains(expectedSupportedType, actualSupportedTypes);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static IEnumerable<object[]> _expectedSupportedTypes;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> ExpectedSupportedTypes
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    IEnumerable<object> GetOne(params Type[] types)
                        => types.Select(type => (object) type);

                    yield return GetOne(
                        typeof(AssemblyVersionAttribute)
                        , typeof(AssemblyFileVersionAttribute)
                        , typeof(AssemblyInformationalVersionAttribute)).ToArray();
                }

                return _expectedSupportedTypes ?? (_expectedSupportedTypes = GetAll().ToArray());
            }
        }

        /// <summary>
        /// Provides some <see cref="IVersionProvider"/> vetting the tests.
        /// </summary>
        /// <inheritdoc />
        private class VersionProviderSamenessComparer : EqualityComparer<IVersionProvider>
        {
            public override bool Equals(IVersionProvider x, IVersionProvider y)
                => !(x == null || y == null) && ReferenceEquals(x, y);

            public override int GetHashCode(IVersionProvider obj)
                => (obj?.Id ?? Guid.Empty).GetHashCode();
        }

        /// <summary>
        /// Returns a Created Fixture. Should not throw any exceptions, but we do not need to test
        /// for this explicitly. Running successfully through passing is sufficient evidence along
        /// these lines.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="versionProviders"></param>
        /// <returns></returns>
        internal static IStreamBumpVersionService CreateFixture(
            ServiceMode mode, params IVersionProvider[] versionProviders)
        {
            var serviceFixture = new StreamBumpVersionServiceFixture<T>(versionProviders) {Mode = mode};

            VerifyFixture(serviceFixture, mode, versionProviders);

            IEnumerable<Action<Regex>> GetExpectedRegexVerification()
            {
                bool ShortNameRegexExpected() => FixtureAttributeType.Name.EndsWith(nameof(Attribute));

                if (ShortNameRegexExpected())
                {
                    yield return regex =>
                    {
                        Assert.NotNull(regex);
                        Assert.Equal(Compiled, regex.Options);
                        Assert.Contains($"{FixtureAttributeType.ToShortName()}", $"{regex}");
                    };
                }

                yield return regex =>
                {
                    Assert.NotNull(regex);
                    Assert.Equal(Compiled, regex.Options);
                    Assert.Contains($"{FixtureAttributeType.ToLongName()}", $"{regex}");
                };
            }

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
        /// Verifies that the <see cref="Regex"/> matches.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="given"></param>
        /// <param name="expectedVersion"></param>
        /// <param name="shouldMatch"></param>
        [Theory
         , MemberData(nameof(DefaultTestCases))]
        public void Verify_Regex_Match(ServiceMode mode, string given, string expectedVersion, bool shouldMatch)
        {
            // TODO: TBD: need a sister unit tests in order to vet at least one or a couple of unsupported attribute examples...
            var expectedProviders = new[] {NoOp, NoOp, NoOp, NoOp};

            var fixture = CreateFixture(mode, expectedProviders);

            Assert.Equal(shouldMatch, fixture.AttributeRegexes
                .FilterMatch(given, shouldMatch)
                .TryVerifyMatch(given, expectedVersion, shouldMatch));
        }

        // ReSharper disable once StaticMemberInGenericType
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
                    => $"[assembly: {FixtureAttributeType.ToShortName()}(\"{version}\")]";

                string GetLongNameUsage(string version)
                    => $"[assembly: {FixtureAttributeType.ToLongName()}(\"{version}\")]";

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

                    // TODO: TBD: just besides these obvious cases, may build cases from them... we could more extensively generate these, but I'm not sure I want to generate thousands of test cases...
                    foreach (var testCase in GetKnownMatches(true
                        , "123.123"
                        , "123.123.*"
                        , "123.123.123"
                        , "123.123.123.*"
                        , "123.123.123.123"
                    ))
                    {
                        yield return testCase;
                    }

                    foreach (var testCase in GetKnownMatches(false
                        , "..."
                        , ".-+"
                        , "1234567890.abcdefghijklmnopqrstuvwxyz..ABCDEFGHIJKLMNOPQRSTUVWXYZ-+*"
                        , "123.123.*.*"
                        , "123.123.123.123.*"
                        , "123.123.123.123.123"
                        , "123.123.123.123.123.*"
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
