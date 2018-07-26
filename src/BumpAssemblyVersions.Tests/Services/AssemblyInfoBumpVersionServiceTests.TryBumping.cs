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

                    /* The aim here is not to prepare an exhaustive combinatorial involving the
                     * Version Providers, but rather to yield several representative examples, but
                     * which also demonstrate that integrated features are in fact also working as
                     * expected. */

                    var dayVersionProvider = (IVersionProvider) Registry.Day;
                    var monthVersionProvider = (IVersionProvider) Registry.Month;
                    var yearVersionProvider = (IVersionProvider) Registry.Year;
                    var shortYearVersionProvider = (IVersionProvider) Registry.ShortYear;
                    //IVersionProvider dayOfYearVersionProvider = Registry.DayOfYear;
                    //IVersionProvider deltaDays1900VersionProvider = Registry.DeltaDays1900;
                    //IVersionProvider deltaDays1970VersionProvider = Registry.DeltaDays1970;
                    //IVersionProvider deltaDays1980VersionProvider = Registry.DeltaDays1980;
                    //IVersionProvider deltaDays1990VersionProvider = Registry.DeltaDays1990;
                    //IVersionProvider deltaDays2000VersionProvider = Registry.DeltaDays2000;
                    //IVersionProvider deltaDays2010VersionProvider = Registry.DeltaDays2010;
                    var incrementVersionProvider = (IVersionProvider) Registry.Increment;
                    var preReleaseIncrementVersionProvider
                        = (PreReleaseIncrementVersionProvider) Registry.PreReleaseIncrement;
                    //IVersionProvider secondsSinceMidnightVersionProvider = Registry.SecondsSinceMidnight;
                    var hourMinuteVersionProvider = (IVersionProvider) Registry.HourMinute;
                    var monthDayOfMonthVersionProvider = (IVersionProvider) Registry.MonthDayOfMonth;
                    //IVersionProvider yearDayOfYearVersionProvider = Registry.YearDayOfYear;
                    //IVersionProvider shortYearDayOfYearVersionProvider = Registry.ShortYearDayOfYear;

                    const bool mayNotCreateNew = false;
                    const bool mayNotIncludeWildcard = false;

                    const char dot = '.';
                    const char hyp = '-';

                    IVersionProvider CloneProvider<TProvider>(TProvider provider, Action<TProvider> init = null)
                        where TProvider : class, IVersionProvider
                    {
                        var clone = (TProvider) provider.Clone();
                        init?.Invoke(clone);
                        return clone;
                    }

                    // The service should respect the given Name, whether Shorthand or Longhand.
                    foreach (var name in GetEnumeratedValues(FixtureAttributeType.ToShortName()
                        , FixtureAttributeType.ToLongName()))
                    {
                        foreach (var mode in GetServiceModes())
                        {
                            switch (mode)
                            {
                                case VersionElements:

                                    const int one = 1;

                                    yield return GetOne(mode
                                        , $@"// Simple Increment example: One, Plus One, Plus Two, Increment
{BuildVersionAttribUsage(name, $"{one}{dot}{one + 1}{dot}{one + 2}{dot}{one}")}
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Simple Increment example: One, Plus One, Plus Two, Increment
{BuildVersionAttribUsage(name, $"{one}{dot}{one + 1}{dot}{one + 2}{dot}{one + 1}")}
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , buildProviderTemplate: incrementVersionProvider).ToArray();

                                    yield return GetOne(mode
                                        , $@"// Date/time based example: Year, Month, Day
{BuildVersionAttribUsage(name, $"{then.Year}{dot}{then.Month}{dot}{then.Day}")}
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Date/time based example: Year, Month, Day
{BuildVersionAttribUsage(name, $"{now.Year}{dot}{now.Month}{dot}{now.Day}")}
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , yearVersionProvider, monthVersionProvider, dayVersionProvider).ToArray();

                                    const int zero = 0;
                                    const int hundred = 100;

                                    yield return GetOne(mode
                                        , $@"// Date/time based example: Short Year, Month Day of Month, Hour Minute, One (Should Reset to Zero)
{
                                                BuildVersionAttribUsage(name,
                                                    $"{then.Year % hundred:D02}{dot}{then.Month:D02}{then.Day:D02}{dot}{then.Hour:D02}{then.Minute:D02}{dot}{one}")
                                            }
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Date/time based example: Short Year, Month Day of Month, Hour Minute, One (Should Reset to Zero)
{
                                                BuildVersionAttribUsage(name,
                                                    $"{now.Year % hundred:D02}{dot}{now.Month:D02}{now.Day:D02}{dot}{now.Hour:D02}{now.Minute:D02}{dot}{zero}")
                                            }
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , shortYearVersionProvider
                                        , monthDayOfMonthVersionProvider, hourMinuteVersionProvider
                                        , CloneProvider(incrementVersionProvider, x => x.MayReset = true)).ToArray();

                                    break;

                                case VersionAndReleaseElements:

                                    const int majorValue = 1;
                                    const int minorValue = 2;
                                    const int patchValue = 34;
                                    const int buildValue = 567;

                                    const string rc = nameof(rc);
                                    const string beta = nameof(beta);

                                    const int preReleaseOne = 1;
                                    const int preReleaseValue = 2;

                                    yield return GetOne(mode
                                        , $@"// Version and release elements example from RC to next RC
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue, buildValue)}{hyp}{rc}{preReleaseValue}")
                                            }
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Version and release elements example from RC to next RC
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue + 1, buildValue + 1)}{hyp}{rc}{preReleaseValue + 1}")
                                            }
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , patchProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , buildProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , releaseProviderTemplate: CloneProvider(
                                            preReleaseIncrementVersionProvider
                                            , privp =>
                                            {
                                                privp.Label = rc;
                                                privp.ValueWidth = 1;
                                            })).ToArray();

                                    yield return GetOne(mode
                                        , $@"// Version and release elements example from RC to RC (reset)
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue, buildValue)}{hyp}{rc}{preReleaseValue}")
                                            }
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Version and release elements example from RC to RC (reset)
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue + 1, buildValue + 1)}{hyp}{rc}{preReleaseOne}")
                                            }
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , patchProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , buildProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , releaseProviderTemplate: CloneProvider(
                                            preReleaseIncrementVersionProvider
                                            , privp =>
                                            {
                                                privp.MayReset = true;
                                                privp.Label = rc;
                                                privp.ValueWidth = 1;
                                            })).ToArray();

                                    yield return GetOne(mode
                                        , $@"// Version and release elements example from Beta to RC
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue, buildValue)}{hyp}{beta}{preReleaseValue + 1}")
                                            }
// Fini"
                                        , $@"using {FixtureAttributeType.Namespace};
// Version and release elements example from Beta to RC
{
                                                BuildVersionAttribUsage(name
                                                    , $"{new Version(majorValue, minorValue, patchValue + 1, buildValue + 1)}{hyp}{rc}{preReleaseOne:D4}")
                                            }
// Fini"
                                        , true, mayNotCreateNew, mayNotIncludeWildcard
                                        , patchProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , buildProviderTemplate: CloneProvider(incrementVersionProvider)
                                        , releaseProviderTemplate: CloneProvider(
                                            preReleaseIncrementVersionProvider
                                            , privp =>
                                            {
                                                privp.Label = rc;
                                                privp.ValueWidth = 4;
                                            })).ToArray();

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
