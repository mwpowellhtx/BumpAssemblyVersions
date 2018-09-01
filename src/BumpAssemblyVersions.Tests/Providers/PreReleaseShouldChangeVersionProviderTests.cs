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
    // ReSharper disable once UnusedMember.Global
    public class PreReleaseShouldChangeVersionProviderTests : VersionProviderTestsBase<
        PreReleaseIncrementVersionProvider>
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
            InitializeProviderCallback<PreReleaseIncrementVersionProvider> init, bool expectedTried)
        {
            var provider = CreateNew();
            init?.Invoke(provider);
            Assert.Equal(expectedTried, provider.TryChange(beforeChange, out var actualAfterChange));
            Assert.Equal(afterChange, actualAfterChange);
        }
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory

        private static IEnumerable<object[]> _beforeAndAfterProviderChanged;

        /// <summary>
        /// Gets the Test Cases.
        /// </summary>
        public static IEnumerable<object[]> BeforeAndAfterProviderChanged
        {
            get
            {
                IEnumerable<object[]> GetAll()
                {
                    const int valueWidth = 5;
                    const string alpha = nameof(alpha);

                    // ReSharper disable once ConvertToLocalFunction
                    InitializeProviderCallback<PreReleaseIncrementVersionProvider> initCallback = p =>
                    {
                        p.Label = alpha;
                        p.MayReset = true;
                        p.ValueWidth = valueWidth;
                        p.MoreSignificantProviders = new[] {new ChangedVersionProvider()};
                    };

                    const int one = 1;

                    IEnumerable<object> GetOne(int current)
                    {
                        var formatString = $@"{{0}}{{1:D0{valueWidth}}}";
                        yield return string.Format(formatString, alpha, current);
                        yield return string.Format(formatString, alpha, one);
                        yield return initCallback;
                        yield return true;
                    }

                    // We want to observe the Reset to One, so do not provision Current as One.
                    for (var i = one + 1; i < one + 4; i++)
                    {
                        yield return GetOne(i).ToArray();
                    }

                    for (var j = MaxValue - 4; j <= MaxValue; j++)
                    {
                        yield return GetOne(j).ToArray();
                    }
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
