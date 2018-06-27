using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;
    using static Math;

    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class PreReleaseIncrementVersionProviderTests : VersionProviderTestsBase<PreReleaseIncrementVersionProvider>
    {

#pragma warning disable xUnit1008 // Test data attribute should only be used on a Theory
        /// <summary>
        /// It is not this Provider&apos;s job to split nor to combine the elements. However,
        /// it is this Provider&apos;s job to increment, clear, etc, the Pre-Release element
        /// according to the configured attributes.
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
                /* There are actually quite a variety of combinations we can capture verifying
                 the transitions, Incrementing from Empty, whether Should Discard, and so on. */

                IEnumerable<object[]> GetAll()
                {
                    const string alpha = nameof(alpha);
                    const string omega = nameof(omega);

                    const int one = 1;
                    const string empty = "";

                    // Value Width used across here is completely arbitrary.
                    const int valueWidth = 5;

                    var valueMax = (int) Pow(10d, valueWidth) - 1;

                    InitializeProviderCallback<PreReleaseIncrementVersionProvider> GetLabeledCallback(string label)
                        => p =>
                        {
                            p.Label = label;
                            p.ValueWidth = valueWidth;
                        };

                    int Increment(int x) => x + 1 > valueMax ? 1 : x + 1;
                    // Be careful with this one. We want to capture the Width, and we want the Current to be Zero Padded.
                    string FormatValue(int current, int width) => string.Format($"{{0:D0{width}}}", current);

                    IEnumerable<object> GetOneDefaultUseCase(string label, int current)
                    {
                        yield return $"{label}{FormatValue(current, valueWidth)}";
                        yield return $"{label}{FormatValue(Increment(current), valueWidth)}";
                        yield return GetLabeledCallback(label);
                        yield return true;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        yield return GetOneDefaultUseCase(alpha, i).ToArray();
                    }

                    for (var j = valueMax - 4; j <= valueMax; j++)
                    {
                        yield return GetOneDefaultUseCase(omega, j).ToArray();
                    }

                    IEnumerable<object> GetFromAnotherLabelUseCase(string a, string b, int current)
                    {
                        yield return $"{a}{FormatValue(current, valueWidth)}";
                        yield return $"{b}{FormatValue(Increment(current), valueWidth)}";
                        yield return GetLabeledCallback(b);
                        yield return true;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        yield return GetFromAnotherLabelUseCase(alpha, omega, i).ToArray();
                    }

                    for (var j = valueMax - 4; j <= valueMax; j++)
                    {
                        yield return GetFromAnotherLabelUseCase(alpha, omega, j).ToArray();
                    }

                    IEnumerable<object> GetFromEmptyUseCase(string label)
                    {
                        yield return empty;
                        yield return $"{label}{FormatValue(one, valueWidth)}";
                        yield return GetLabeledCallback(label);
                        yield return true;
                    }

                    InitializeProviderCallback<PreReleaseIncrementVersionProvider> GetShouldDiscardCallback()
                        => p => p.ShouldDiscard = true;

                    IEnumerable<object> GetShouldDiscardUseCase(string label, int current)
                    {
                        yield return $"{label}{FormatValue(current, valueWidth)}";
                        yield return empty;
                        yield return GetShouldDiscardCallback();
                        yield return true;
                    }

                    foreach (var l in new[] {alpha, omega})
                    {
                        yield return GetFromEmptyUseCase(l).ToArray();

                        for (var i = 0; i < 4; i++)
                        {
                            yield return GetShouldDiscardUseCase(l, i).ToArray();
                        }

                        for (var j = valueMax - 4; j <= valueMax; j++)
                        {
                            yield return GetShouldDiscardUseCase(l, j).ToArray();
                        }
                    }

                    IEnumerable<object> GetDoesNotChangeUseCase()
                    {
                        yield return empty;
                        yield return empty;
                        yield return GetShouldDiscardCallback();
                        yield return false;
                    }

                    yield return GetDoesNotChangeUseCase().ToArray();
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
