using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class NoOpVersionProviderTests : VersionProviderTestsBase<NoOpVersionProvider>
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
        [MemberData(nameof(BeforeAndAfterProviderChanged))]
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange, string afterChange,
            InitializeProviderCallback<NoOpVersionProvider> init, bool expectedTried)
        {
            var provider = CreateNew();
            init?.Invoke(provider);
            Assert.Equal(expectedTried, provider.TryChange(beforeChange, out var actualAfterChange));
            Assert.Equal(afterChange, actualAfterChange);
        }
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory

        private static IEnumerable<object[]> _beforeAndAfterProviderChanged;

        private static InitializeProviderCallback<NoOpVersionProvider> GetProviderInitializationCallback() => p => { };

        /// <summary>
        /// Gets the Test Cases.
        /// </summary>
        public static IEnumerable<object[]> BeforeAndAfterProviderChanged
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    IEnumerable<object> GetOne(string s)
                    {
                        yield return s;
                        yield return s;
                        yield return GetProviderInitializationCallback();
                        yield return false;
                    }

                    const int one = 1;

                    IEnumerable<int> GetShifts(int start = one)
                    {
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start;
                    }

                    foreach (var shift in GetShifts())
                    {
                        yield return GetOne($"{one << shift}").ToArray();
                    }

                    /* Although we're talking about numeric Version elements in most cases,
                     there are a couple that should ideally also support Alphanumeric. */
                    IEnumerable<char> GetChars(char start)
                    {
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start++;
                        yield return start;
                    }

                    const char a = 'a';

                    foreach (var ch in GetChars(a))
                    {
                        yield return GetOne($"{ch}").ToArray();
                    }

                    /* Although we're talking about numeric Version elements in most cases,
                     there are a couple that should ideally also support Alphanumeric. */
                    IEnumerable<string> GetAlphanumeric(char ch, int shifts = one)
                    {
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch++}{(1 << shifts++):D03}";
                        // ReSharper disable once ArrangeRedundantParentheses
                        yield return $"{ch}{(1 << shifts):D03}";
                    }

                    foreach (var alphanumeric in GetAlphanumeric(a))
                    {
                        yield return GetOne($"{alphanumeric}").ToArray();
                    }
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
