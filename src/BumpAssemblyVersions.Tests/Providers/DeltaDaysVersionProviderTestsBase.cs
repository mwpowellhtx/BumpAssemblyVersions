using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using static Int16;
    using static Math;
    using static Type;
    using static BindingFlags;


    /// <summary>
    /// Much of these unit tests are tucked away in the form of common, repeatable baseline
    /// patterns.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    public abstract class DeltaDaysVersionProviderTestsBase<T> : TimestampVersionProviderTestsBase<T>
        where T : DeltaDaysVersionProviderBase
    {
        /// <summary>
        /// Returns the <see cref="DateTime"/> baed on the
        /// <see cref="DeltaDaysVersionProviderBase"/>.
        /// </summary>
        /// <returns></returns>
        protected static DateTime GetBaseTimestampFromProvider()
        {
            var method = ProviderType.GetMethod("GetBaseTimestamp", Static | NonPublic, DefaultBinder, new Type[] { }, null);
            Assert.NotNull(method);
            Assert.True(method.IsPrivate);
            Assert.Equal(typeof(DateTime), method.ReturnType);
            var obj = method.Invoke(null, new object[] { });
            Assert.IsType<DateTime>(obj);
            return (DateTime) obj;
        }

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
            InitializeProviderCallback<T> init, bool expectedTried)
        {
            base.Verify_Version_element_After_Trying_to_Change(beforeChange, afterChange, init, expectedTried);
        }
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory

        // ReSharper disable once StaticMemberInGenericType
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
                    int CalculateDeltaDays(DateTime first, DateTime second, int offset = 1)
                        => (Abs((second - first).Days) + offset) % MaxValue;

                    var now = DateTime.Now;

                    IEnumerable<object> GetOne(string before, string after)
                    {
                        yield return before;
                        yield return after;
                        yield return GetProviderInitializationCallback(now);
                        yield return before != after;
                    }

                    const int zed = 0;

                    var baseTimestamp = GetBaseTimestampFromProvider();

                    yield return GetOne($"{CalculateDeltaDays(baseTimestamp, now, zed)}"
                        , $"{CalculateDeltaDays(baseTimestamp, now)}").ToArray();

                    yield return GetOne($"{CalculateDeltaDays(baseTimestamp, now)}"
                        , $"{CalculateDeltaDays(baseTimestamp, now)}").ToArray();
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
