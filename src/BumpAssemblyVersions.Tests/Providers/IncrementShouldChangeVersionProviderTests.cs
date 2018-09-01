using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;
    using static Int16;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class IncrementShouldChangeVersionProviderTests : VersionProviderTestsBase<IncrementVersionProvider>
    {

#pragma warning disable xUnit1008 // Test data attribute should only be used on a Theory
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeChange"></param>
        /// <param name="afterChange"></param>
        /// <param name="init"></param>
        /// <param name="expectedTried"></param>
        /// <inheritdoc />
        [MemberData(nameof(BeforeAndAfterProviderChange))]
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange, string afterChange,
            InitializeProviderCallback<IncrementVersionProvider> init, bool expectedTried)
        {
            var provider = CreateNew();
            init?.Invoke(provider);
            // Throws nothing simply allows it to happen bereft of test framework involvement.
            Assert.Equal(expectedTried, provider.TryChange(beforeChange, out var actualAfterChange));
            Assert.Equal(afterChange, actualAfterChange);
        }
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory

        private static IEnumerable<object[]> _beforeAndAfterProviderChange;

        private static InitializeProviderCallback<IncrementVersionProvider> GetShouldChangeInitializationCallback(
            params bool?[] changed)
        {
            IEnumerable<IVersionProvider> GetMoreSignificantProviders()
            {
                foreach (var x in changed)
                {
                    yield return new ChangedVersionProvider(x);
                }
            }

            return p =>
            {
                p.MayReset = true;
                p.MoreSignificantProviders = GetMoreSignificantProviders().ToArray();
            };
        }

        /// <summary>
        /// Gets the Test Cases.
        /// </summary>
        public static IEnumerable<object[]> BeforeAndAfterProviderChange
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    int Increment(int x) => x >= MaxValue ? 0 : x + 1;

                    IEnumerable<object> GetShouldChange(int x, params bool?[] changed)
                    {
                        const int zed = 0;
                        var y = changed.Any() && changed.Any(z => z == true) ? zed : Increment(x);
                        yield return $"{x}";
                        yield return $"{y}";
                        yield return GetShouldChangeInitializationCallback(changed);
                        // Not whether Y was equal to Zed, but did it actually Change from X.
                        yield return y != x;
                    }

                    /* May get more detailed, more exhaustive, if possible, such as testing when there are
                     two or more More Significant Providers and in what combinations of having Changed... */
                    for (var i = 0; i < 4; i++)
                    {
                        // However, this is enough to capture the essence, I think.
                        yield return GetShouldChange(i, true).ToArray();
                        yield return GetShouldChange(i, false).ToArray();
                    }

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    for (int j = MaxValue - 4; j <= MaxValue; j++)
                    {
                        yield return GetShouldChange(j, true).ToArray();
                        yield return GetShouldChange(j, false).ToArray();
                    }
                }

                return _beforeAndAfterProviderChange ?? (_beforeAndAfterProviderChange = GetAll().ToArray());
            }
        }
    }
}
