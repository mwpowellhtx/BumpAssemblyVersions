using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using static DateTime;
    using static ProviderExtensionMethods;
    using static String;
    using static TimeSpan;
    using static VersionProviderTemplateRegistry;
    using static ServiceMode;
    using static VersionKind;

    // TODO: TBD: these are not simply "regex" tests any longer, so rename away from that in a subsequent near future commit...
    // ReSharper disable once UnusedTypeParameter
    public abstract partial class AssemblyInfoBumpVersionServiceTests<T>
    {
        /// <summary>
        /// Verifies the Try Bump functionality.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="givenSource"></param>
        /// <param name="expectedSource"></param>
        /// <param name="expectedTrySuccess"></param>
        /// <param name="descriptor"></param>
        [Theory
         , MemberData(nameof(TryBumpTestCases))]
        public void Verify_Try_Bump(ServiceMode mode, string givenSource
            , string expectedSource, bool expectedTrySuccess
            , IBumpVersionDescriptor descriptor)
        {
            // TODO: TBD: split the lines here approaching the test? or within the test itself?
            IEnumerable<string> SplitLines(string s = null)
                => (s ?? Empty).Replace("\r\n", "\n").Split('\n');

            string CombineLines(IEnumerable<string> lines)
                => Join("\r\n", lines);

            // TODO: TBD: this is it, I think, in a nutshell; notwithstanding nuances...
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.VersionProviderTemplates);
            Assert.Collection(descriptor.VersionProviderTemplates
                , p => { Assert.NotNull(p); }
                , p => { Assert.NotNull(p); }
                , p => { Assert.NotNull(p); }
                , p => { Assert.NotNull(p); }
                , p => { Assert.NotNull(p); }
                );
            var service = CreateServiceFixture(mode, descriptor);
            var givenLines = SplitLines(givenSource).ToArray();
            var actualTrySuccess = service.TryBumpVersion(givenLines, out var actualLines);
            Assert.Equal(expectedTrySuccess, actualTrySuccess);
            var actualSource = CombineLines(actualLines);
            Assert.Equal(expectedSource, actualSource);
        }

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        /// 
        /// </summary>
        private static IEnumerable<object[]> _tryBumpTestCases;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> TryBumpTestCases
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    var now = Now;
                    FilterCallback providerFilter = ProviderExtensionMethods.Filter;
                    const int secondsInMinute = 60;
                    const int minutesInHour = 60;
                    const int threeHoursInSeconds = 3 * secondsInMinute * minutesInHour + 17 * minutesInHour + 15;

                    string BuildVersionAttribUsage(string name, string version) => $"[assembly: {name}(\"{version}\")]";

                    IEnumerable<ServiceMode> GetServiceModes()
                    {
                        yield return VersionElements;
                        yield return ReleaseElements;
                        yield return VersionAndReleaseElements;
                        yield return MetadataElements;
                    }

                    IEnumerable<TValue> GetEnumeratedValues<TValue>(params TValue[] values) => values;

                    VersionKind InterpolateKind()
                    {
                        if (typeof(T) == typeof(AssemblyVersionAttribute))
                        {
                            return AssemblyVersion;
                        }

                        if (typeof(T) == typeof(AssemblyInformationalVersionAttribute))
                        {
                            return AssemblyInformationVersion;
                        }

                        // ReSharper disable once ConvertIfStatementToReturnStatement
                        if (typeof(T) == typeof(AssemblyFileVersionAttribute))
                        {
                            return AssemblyFileVersion;
                        }

                        return None;
                    }

                    /* Alignment of the Descriptor Kind is really where we might expect some
                     * sort of change to occur with respect to the Tried Bump operation. */
                    IBumpVersionDescriptor GetDescriptor(VersionKind kind
                        , bool mayCreateNew = false, bool mayIncludeWildcard = false
                        , IVersionProvider majorProviderTemplate = null
                        , IVersionProvider minorProviderTemplate = null
                        , IVersionProvider patchProviderTemplate = null
                        , IVersionProvider buildProviderTemplate = null
                        , IVersionProvider releaseProviderTemplate = null
                    )
                    {
                        var descriptor = new BumpVersionDescriptorFixture(
                            majorProviderTemplate, minorProviderTemplate
                            , patchProviderTemplate, buildProviderTemplate
                            , releaseProviderTemplate
                        )
                        {
                            Kind = kind,
                            CreateNew = mayCreateNew,
                            IncludeWildcard = mayIncludeWildcard,
                            InternalDescriptorTimestamp = now
                        };
                        return descriptor;
                    }

                    // TODO: TBD: this really has to start looking more like an "integration" test; of what?
                    // TODO: TBD: of service with descriptor, including consideration of bits such as IncludeWildcard, CreateNew, and so on

                    /* TODO: TBD: quite a bit left to connect here, including, but not limited to, the Test Theory parameters,
                     * but the building blocks are here, I think, to consider what the test cases need to start looking like... */
                    IEnumerable<object> GetOne(ServiceMode mode, string givenSource, string expectedSource
                        , bool expectedTrySuccess, bool mayCreateNew = false, bool mayIncludeWildcard = false
                        , IVersionProvider majorProviderTemplate = null, IVersionProvider minorProviderTemplate = null
                        , IVersionProvider patchProviderTemplate = null, IVersionProvider buildProviderTemplate = null
                        , IVersionProvider releaseProviderTemplate = null
                    )
                    {
                        yield return mode;
                        yield return givenSource ?? Empty;
                        yield return expectedSource ?? Empty;
                        yield return expectedTrySuccess;
                        yield return GetDescriptor(InterpolateKind(), mayCreateNew, mayIncludeWildcard
                            , majorProviderTemplate, minorProviderTemplate, patchProviderTemplate
                            , buildProviderTemplate, releaseProviderTemplate);
                    }

                    providerFilter(Registry.NoOp);

                    // Ensure that any Timestamp based Versions would in fact be Different.
                    DateTime EnsureUniqueTimestamp(DateTime timestamp, int deltaDays = 431
                        , int deltaSeconds = threeHoursInSeconds)
                    {
                        // Starting from Base Timestamp.
                        var candidate = timestamp;
                        var delta = FromDays(deltaDays) + FromSeconds(deltaSeconds);
                        do
                        {
                            // Subtracting out the Days Plus Seconds.
                            candidate -= delta;
                            // Rinse and Repeat Until All of the Constituent Members are Unique.
                        } while (candidate.Year == timestamp.Year
                                 || candidate.Month == timestamp.Month
                                 || candidate.Day == timestamp.Day
                                 || candidate.DayOfYear == timestamp.DayOfYear
                                 || candidate.Hour == timestamp.Hour
                                 || candidate.Minute == timestamp.Minute
                                 || candidate.Second == timestamp.Second);

                        return candidate;
                    }

                    var then = EnsureUniqueTimestamp(now);

                    /* The aim here is not to prepare an exhaustive combinatorial involving the Version Providers,
                     but rather to yield several representative examples, but which also demonstrate that integrated
                     features are in fact also working as expected. */

                    IVersionProvider dayVersionProvider = Registry.Day;
                    IVersionProvider monthVersionProvider = Registry.Month;
                    IVersionProvider yearVersionProvider = Registry.Year;
                    //IVersionProvider shortYearVersionProvider = Registry.ShortYear;
                    //IVersionProvider dayOfYearVersionProvider = Registry.DayOfYear;
                    //IVersionProvider deltaDays1900VersionProvider = Registry.DeltaDays1900;
                    //IVersionProvider deltaDays1970VersionProvider = Registry.DeltaDays1970;
                    //IVersionProvider deltaDays1980VersionProvider = Registry.DeltaDays1980;
                    //IVersionProvider deltaDays1990VersionProvider = Registry.DeltaDays1990;
                    //IVersionProvider deltaDays2000VersionProvider = Registry.DeltaDays2000;
                    //IVersionProvider deltaDays2010VersionProvider = Registry.DeltaDays2010;
                    //IVersionProvider incrementVersionProvider = Registry.Increment;
                    //IVersionProvider preReleaseIncrementVersionProvider = Registry.PreReleaseIncrement;
                    //IVersionProvider secondsSinceMidnightVersionProvider = Registry.SecondsSinceMidnight;
                    //IVersionProvider hourMinuteVersionProvider = Registry.HourMinute;
                    //IVersionProvider monthDayOfMonthVersionProvider = Registry.MonthDayOfMonth;
                    //IVersionProvider yearDayOfYearVersionProvider = Registry.YearDayOfYear;
                    //IVersionProvider shortYearDayOfYearVersionProvider = Registry.ShortYearDayOfYear;

                    const bool mayNotCreateNew = false;
                    const bool mayNotIncludeWildcard = false;

                    // The service should respect the given Name, whether Shorthand or Longhand.
                    foreach (var name in GetEnumeratedValues(FixtureAttributeType.ToShortName()
                        , FixtureAttributeType.ToLongName()))
                    {
                        foreach (var mode in GetServiceModes())
                        {
                            switch (mode)
                            {
                                case VersionElements:
                                    yield return GetOne(mode
                                        , $@"// Example
{BuildVersionAttribUsage(name, $"{then.Year}.{then.Month}.{then.Day}")}
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Example
{BuildVersionAttribUsage(name, $"{now.Year}.{now.Month}.{now.Day}")}
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , yearVersionProvider, monthVersionProvider, dayVersionProvider).ToArray();
                                    break;
                            }
                        }
                    }
                }

                return _tryBumpTestCases ?? (_tryBumpTestCases = GetAll().ToArray());
            }
        }
    }
}
