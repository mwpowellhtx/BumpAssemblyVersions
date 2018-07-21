using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using Xunit;
    using static RegexOptions;
    using static ServiceMode;

    public abstract partial class BumpVersionServiceAttributeRegexTests<T>
        where T : Attribute
    {
        /// <summary>
        /// Verifies that the <see cref="Regex"/> matches.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="given"></param>
        /// <param name="expectedVersion"></param>
        /// <param name="shouldMatch"></param>
        [Theory
         , MemberData(nameof(RegexMatchTestCases))]
        public void Verify_Regex_Match(ServiceMode mode, string given, string expectedVersion, bool shouldMatch)
        {
            void VerifyServiceRegexes(IAssemblyInfoBumpVersionService service)
            {
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
                Assert.Collection(service.AttributeRegexes, GetExpectedRegexVerification().ToArray());
            }

            var fixture = CreateFixture(mode, VerifyServiceRegexes);

            Assert.Equal(shouldMatch, fixture.AttributeRegexes
                .FilterMatch(given, shouldMatch)
                .TryVerifyMatch(given, expectedVersion, shouldMatch));
        }

        // ReSharper disable once StaticMemberInGenericType
        private static IEnumerable<object[]> _regexMatchTestCases;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> RegexMatchTestCases
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

                    // TODO: TBD: just besides these obvious cases, may build cases from them...
                    // TODO: TBD: we could more extensively generate these, but I'm not sure I want to generate thousands of test cases...
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

                return _regexMatchTestCases ?? (_regexMatchTestCases = GetAll().ToArray());
            }
        }
    }
}
