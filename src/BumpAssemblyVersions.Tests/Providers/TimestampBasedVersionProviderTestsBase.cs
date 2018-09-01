using System;

namespace Bav
{
    using Xunit;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    public abstract class TimestampVersionProviderTestsBase<T> : VersionProviderTestsBase<T>
        where T : VersionProviderBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeChange"></param>
        /// <param name="afterChange"></param>
        /// <param name="init"></param>
        /// <param name="expectedTried"></param>
        /// <inheritdoc />
        public override void Verify_Version_element_After_Trying_to_Change(string beforeChange
            , string afterChange, InitializeProviderCallback<T> init, bool expectedTried)
        {
            var provider = CreateNew();
            init?.Invoke(provider);
            Assert.Equal(expectedTried, provider.TryChange(beforeChange, out var actualAfterChange));
            Assert.Equal(afterChange, actualAfterChange);
        }

        /// <summary>
        /// Gets the Callback designed by the <paramref name="timestamp"/>. Effects
        /// the <see cref="VersionProviderBase.Timestamp"/> property.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        protected static InitializeProviderCallback<T> GetProviderInitializationCallback(DateTime timestamp)
            => p => p.Timestamp = timestamp;
    }
}
