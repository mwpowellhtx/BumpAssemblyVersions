using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using Xunit;
    using static String;

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public class UnknownVersionProviderTests : VersionProviderTestsBase<UnknownVersionProvider>
    {

#pragma warning disable xUnit1008 // Test data attribute should only be used on a Theory
        /// <summary>
        /// Verification does not actually do anything. In this instance we actually do
        /// expect to find a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="beforeChange"></param>
        /// <param name="afterChange"></param>
        /// <param name="init"></param>
        /// <param name="expectedTried"></param>
        /// <see cref="NotImplementedException"/>
        /// <inheritdoc />
        [MemberData(nameof(BeforeAndAfterProviderChanged))]
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange
            , string afterChange, InitializeProviderCallback<UnknownVersionProvider> init, bool expectedTried)
        {
            var provider = CreateNew();

            Assert.Throws<NotImplementedException>(() => provider.TryChange(beforeChange, out var x));
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
                InitializeProviderCallback<UnknownVersionProvider> initNull = null;

                IEnumerable<object[]> GetAll()
                {
                    // ReSharper disable once ExpressionIsAlwaysNull
                    yield return new object[] {Empty, Empty, initNull, false};
                }

                return _beforeAndAfterProviderChanged ?? (_beforeAndAfterProviderChanged = GetAll().ToArray());
            }
        }
    }
}
