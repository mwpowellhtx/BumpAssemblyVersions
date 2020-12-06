using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;
    using static String;
    using static Type;
    using static VersionProviderTemplateRegistry;
    using static BindingFlags;

    /// <summary>
    /// 
    /// </summary>
    public class VersionProviderTemplateRegistryTests
    {
        private ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        public VersionProviderTemplateRegistryTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// Verifies that the Registry is correct.
        /// </summary>
        [Fact]
        public void VerifyRegistry()
        {
            bool TryGetRegistry(out IDictionary<string, object> x)
            {
                var y = Registry;
                Assert.NotNull(y);
                x = Assert.IsAssignableFrom<IDictionary<string, object>>(y);
                return x != null;
            }

            Assert.True(TryGetRegistry(out var registry));

            Assert.IsAssignableFrom<ExpandoObject>(registry);

            /* We can verify this deeper, but, if we have assurance that the Dictionary
             * keyed on Version Provider Name works, then that is sufficient. */
            var dictionary = registry.ToDictionary(provider => Assert.IsAssignableFrom<IVersionProvider>(provider.Value).Name);

            Assert.NotNull(dictionary);

            Assert.Equal(dictionary.Count, registry.Count);
        }

        private static void VerifyProviderObject(Type providerType, object providerObject
            , bool expectedChanged = false, bool expectedMayReset = false)
        {
            Assert.NotNull(providerObject);
            Assert.IsAssignableFrom<IVersionProvider>(providerObject);
            Assert.IsAssignableFrom<ICloneable>(providerObject);
            Assert.IsType(providerType, providerObject);
            var provider = providerObject as IVersionProvider;
            Assert.NotNull(provider);
            Assert.NotNull(provider.Name);
            Assert.NotEmpty(provider.Name);
            Assert.Equal(expectedChanged, provider.Changed);
            Assert.False(provider.UseUtc);
            Assert.Equal(expectedMayReset, provider.MayReset);
            Assert.NotEqual(Guid.Empty, provider.Id);
            // The Unknown provider is a bit of a special child...
            Assert.Equal(providerObject is UnknownVersionProvider, provider.ForInternalUseOnly);
            Assert.Null(provider.MoreSignificantProviders);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerType"></param>
        [Theory, MemberData(nameof(ProviderTypeTestCases))]
        public void Version_Provider_has_functional_NonPublic_default_Ctor(Type providerType)
        {
            Assert.NotNull(providerType);
            var ctor = providerType.GetConstructor(Instance | NonPublic, DefaultBinder, new Type[] { }, null);
            Assert.NotNull(ctor);
            var providerObject = ctor.Invoke(new object[] { });
            VerifyProviderObject(providerType, providerObject);
            var providerClone = ((ICloneable) providerObject).Clone();
            VerifyProviderObject(providerType, providerClone);
        }

        private static IEnumerable<object[]> _providerTypeTestCases;

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<object[]> ProviderTypeTestCases
            => _providerTypeTestCases
               ?? (_providerTypeTestCases = GetProviderTypes().Select(x => new object[] {x}));

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Registry_has_all_distinct_Names()
        {
            var providers = GetProviders().ToArray();
            var grouped = providers.GroupBy(provider => provider.Name).ToArray();
            try
            {
                Assert.Equal(providers.Length, grouped.Length);
            }
            catch (EqualException)
            {
                foreach (var duplicate in grouped.Where(g => g.Count() > 1))
                {
                    OutputHelper.WriteLine($"Duplicate: \"{duplicate.First().Name}\"");
                }

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Registry_is_valid()
        {
            var providers = Providers;
            Assert.NotEmpty(providers);
        }

        /// <summary>
        /// The Callback Delegate is used as a way of connecting the dots between
        /// the test cases and the Actual <see cref="Registry"/>.
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public delegate IVersionProvider GetVersionProviderFromDynamicCallback(dynamic registry);

        /// <summary>
        /// Verifies that the <see cref="Registry"/> has Valid Dynamic fields by the name
        /// of <paramref name="registeredName"/>. The <paramref name="getter"/> provides
        /// a sort of handshake that we may utilize in order to further verify whether
        /// this is the case.
        /// </summary>
        /// <param name="registeredName"></param>
        /// <param name="getter"></param>
        [Theory, MemberData(nameof(ValidDynamicFieldTestCases))]
        public void Registry_has_valid_Dynamic_Fields(string registeredName, GetVersionProviderFromDynamicCallback getter)
        {
            OutputHelper.WriteLine($"Verifying Registry has '{registeredName}' '{typeof(IVersionProvider)}'.");
            Assert.NotNull(Registry);
            Assert.True(Registry is ExpandoObject);
            var dictionary = (IDictionary<string, object>) (ExpandoObject) Registry;
            Assert.True(dictionary.ContainsKey(registeredName));
            /* All of them must be IVersionProvider. In addition, some of them may
             * be Multipart, but we do not really care about that at this level. */
            var registeredProvider = dictionary[registeredName] as IVersionProvider;
            Assert.NotNull(registeredProvider);
            var providerType = registeredProvider.GetType();
            OutputHelper.WriteLine($"Type of Registered Provider is '{providerType.FullName}'");
            // This is a sort of internal handshake that we indeed have the correct Expected instances.
            Assert.StartsWith(registeredName, providerType.Name);
            var actualProvider = getter(Registry);
            Assert.Same(registeredProvider, actualProvider);
        }

        private static IEnumerable<object[]> _validDynamicFieldTestCases;

        /// <summary>
        /// Gets the Test Cases.
        /// </summary>
        public static IEnumerable<object[]> ValidDynamicFieldTestCases
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    GetVersionProviderFromDynamicCallback GetVersionProviderCallback(string key)
                        => registry =>
                        {
                            var dictionary = (IDictionary<string, object>) registry;
                            return (dictionary.ContainsKey(key) ? dictionary[key] : null) as IVersionProvider;
                        };

                    IEnumerable<object> GetOne<TProvider, TInterface>()
                        where TInterface : IVersionProvider
                        where TProvider : class, IVersionProvider
                    {
                        // TODO: TBD: consider leveraging bits of the API code for this, would need to be refactored apart from the Registry static class itself...
                        var suffix = typeof(TInterface).Name.Substring(1, typeof(TInterface).Name.Length - 1);
                        var expectedName = typeof(TProvider).Name.Replace(suffix, Empty);
                        yield return expectedName;
                        yield return GetVersionProviderCallback(expectedName);
                    }

                    yield return GetOne<DayVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DayOfYearVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays1900VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays1970VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays1980VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays1990VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays2000VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays2010VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<DeltaDays2020VersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<IncrementVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<MinutesSinceMidnightVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<MonthVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<NoOpVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<PreReleaseIncrementVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<SecondsSinceMidnightVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<ShortYearVersionProvider, IVersionProvider>().ToArray();
                    yield return GetOne<YearVersionProvider, IVersionProvider>().ToArray();

                    yield return GetOne<HourMinuteMultipartVersionProvider, IMultipartVersionProvider>().ToArray();
                    yield return GetOne<MonthDayOfMonthMultipartVersionProvider, IMultipartVersionProvider>().ToArray();
                    yield return GetOne<ShortYearDayOfYearMultipartVersionProvider, IMultipartVersionProvider>().ToArray();
                    yield return GetOne<YearDayOfYearMultipartVersionProvider, IMultipartVersionProvider>().ToArray();
                }

                return _validDynamicFieldTestCases ?? (_validDynamicFieldTestCases = GetAll().ToArray());
            }
        }
    }
}
