using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;
    using static Int16;
    using static Math;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class SecondsSinceMidnightVersionProviderTests : TimestampVersionProviderTestsBase<
        SecondsSinceMidnightVersionProvider>
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
            InitializeProviderCallback<SecondsSinceMidnightVersionProvider> init, bool expectedTried)
        {
            base.Verify_Version_element_After_Trying_to_Change(beforeChange, afterChange, init, expectedTried);
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
                    IEnumerable<object> GetOne(string before, string after, DateTime timestamp)
                    {
                        yield return before;
                        yield return after;
                        yield return GetProviderInitializationCallback(timestamp);
                        yield return before != after;
                    }

                    // This method controls the Adjustment Deltas used throughout Range Calculation.
                    int GetNextAdjustment(double totalSeconds)
                    {
                        const int sixty = 60;
                        const int thirty = 30;
                        const int fifteen = 15;
                        const int five = 5;
                        const int one = 1;
                        const int secondsPerMinute = sixty;

                        if (totalSeconds > sixty * secondsPerMinute)
                        {
                            return -thirty * secondsPerMinute;
                        }

                        if (totalSeconds > fifteen * secondsPerMinute)
                        {
                            return -five * secondsPerMinute;
                        }

                        if (totalSeconds > sixty)
                        {
                            return -fifteen;
                        }

                        return -one;
                    }

                    /* This is the key to the whole thing. We want to calcluate this range as quickly
                     as possible and not be quite as concerned about the Adjustment Deltas. */

                    const int zed = 0;

                    IEnumerable<double> CalculateDeltaSecondsRange(DateTime a, DateTime b)
                    {
                        var delta = Abs((a - b).TotalSeconds);
                        do
                        {
                            yield return (int) delta % MaxValue;
                        } while ((delta += GetNextAdjustment(delta)) > zed);
                    }

                    void InitializeNowAndMidnight(out DateTime nowResult, out DateTime midnightResult)
                    {
                        // Let's always have a consistent data set through Noon O'Clock, regardless of the time Now.
                        nowResult = DateTime.Now;
                        nowResult = DateTime.Parse($"{nowResult:MM/dd/yyyy} 11:59 AM");
                        midnightResult = new DateTime(nowResult.Year, nowResult.Month, nowResult.Day);
                    }

                    InitializeNowAndMidnight(out var now, out var midnight);

                    // And apply Distinct in the event there may possibly be duplicates.
                    foreach (var s in CalculateDeltaSecondsRange(now, midnight).Distinct())
                    {
                        // Now and Then...
                        var nowAndThen = midnight.AddSeconds(s);
                        // From this perspective Midnight is the baseline, not Now.
                        yield return GetOne($"{s}", $"{s}", nowAndThen).ToArray();
                        yield return GetOne($"{s - 1}", $"{s}", nowAndThen).ToArray();
                    }
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
