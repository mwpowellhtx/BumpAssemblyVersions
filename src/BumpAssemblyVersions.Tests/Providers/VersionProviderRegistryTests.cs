using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;
    using static Type;
    using static VersionProviderTemplateRegistry;
    using static BindingFlags;

    /// <summary>
    /// 
    /// </summary>
    public class VersionProviderRegistryTests
    {
        private ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputHelper"></param>
        public VersionProviderRegistryTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
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
    }
}
