using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using Xunit;
    using static String;
    using static Type;
    using static BindingFlags;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    public abstract class MultipartTimestampVersionProviderTestsBase<T> : TimestampVersionProviderTestsBase<T>
        where T : MultipartTimestampVersionProviderBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static IEnumerable<MultipartTimestampCallback> GetMultipartCallbacks()
        {
            var method = ProviderType.GetMethod(nameof(GetMultipartCallbacks), Static | NonPublic
                , DefaultBinder, new Type[] { }, null);
            Assert.NotNull(method);
            Assert.Equal(typeof(IEnumerable<MultipartTimestampCallback>), method.ReturnType);
            var obj = method.Invoke(null, new object[] { });
            Assert.NotNull(obj);
            return (IEnumerable<MultipartTimestampCallback>) obj;
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
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange
            , string afterChange, InitializeProviderCallback<T> init
            , bool expectedTried)
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
                var callbacks = GetMultipartCallbacks().ToArray();

                IEnumerable<object[]> GetAll()
                {
                    IEnumerable<object> GetOne(DateTime a, DateTime b)
                    {
                        var before = Join(Empty, callbacks.Select(callback => callback(a)));
                        var after = Join(Empty, callbacks.Select(callback => callback(b)));

                        yield return before;
                        yield return after;
                        yield return GetProviderInitializationCallback(b);
                        yield return before != after;
                    }

                    IEnumerable<object> GetOneGivenBefore(string before, DateTime b)
                    {
                        var after = Join(Empty, callbacks.Select(callback => callback(b)));

                        yield return before;
                        yield return after;
                        yield return GetProviderInitializationCallback(b);
                        yield return before != after;
                    }

                    const int one = 1;
                    const int three = 3;
                    const int thirty = 30;

                    var now = DateTime.Now;

                    // Then just add the breadth of test cases regardless of the specific Multipart.
                    yield return GetOneGivenBefore(Empty, now).ToArray();
                    yield return GetOne(now, now).ToArray();
                    yield return GetOne(now.AddYears(-one), now).ToArray();
                    yield return GetOne(now.AddMonths(-one), now).ToArray();
                    yield return GetOne(now.AddDays(-(thirty + three)), now).ToArray();
                    yield return GetOne(now.AddDays(-one), now).ToArray();
                    yield return GetOne(now.AddHours(-one), now).ToArray();
                    yield return GetOne(now.AddMinutes(-one), now).ToArray();
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
