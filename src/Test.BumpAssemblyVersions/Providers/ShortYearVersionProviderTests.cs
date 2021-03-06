﻿using System;
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
    public class ShortYearVersionProviderTests : TimestampVersionProviderTestsBase<ShortYearVersionProvider>
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
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange
            , string afterChange, InitializeProviderCallback<ShortYearVersionProvider> init, bool expectedTried)
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
                    var now = DateTime.Now;

                    var shortYear = now.Year % 100;

                    IEnumerable<object> GetOne(string before, string after)
                    {
                        yield return before;
                        yield return after;
                        yield return GetProviderInitializationCallback(now);
                        yield return before != after;
                    }

                    // This could yield a Negative Before Change, but this is fine for test purposes.
                    yield return GetOne($"{shortYear - 1}", $"{shortYear}").ToArray();
                    yield return GetOne($"{shortYear}", $"{shortYear}").ToArray();
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
