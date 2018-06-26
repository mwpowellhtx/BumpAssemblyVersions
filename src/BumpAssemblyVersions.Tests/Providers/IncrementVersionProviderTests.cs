using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;
    using static Int16;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class IncrementVersionProviderTests : VersionProviderTestsBase<IncrementVersionProvider>
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

        private static InitializeProviderCallback<IncrementVersionProvider> GetProviderInitializationCallback() => null;

        /// <summary>
        /// Gets the Test Cases.
        /// </summary>
        public static IEnumerable<object[]> BeforeAndAfterProviderChange
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    int Increment(int y) => y >= MaxValue ? 0 : y + 1;

                    IEnumerable<object> GetOne(int x, int y)
                    {
                        yield return $"{x}";
                        yield return $"{y}";
                        yield return GetProviderInitializationCallback();
                        // We expect that it will always have been Tried.
                        const bool expectedTried = true;
                        yield return expectedTried;
                    }

                    IEnumerable<object> GetIncremented(int x) => GetOne(x, Increment(x));

                    for (var i = 0; i < 4; i++)
                    {
                        yield return GetIncremented(i).ToArray();
                    }

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    for (int j = MaxValue - 4; j <= MaxValue; j++)
                    {
                        yield return GetIncremented(j).ToArray();
                    }
                }

                return _beforeAndAfterProviderChange ?? (_beforeAndAfterProviderChange = GetAll().ToArray());
            }
        }
    }
}
