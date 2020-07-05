using System;

namespace Bav
{
    /// <summary>
    /// Provides a set of <see cref="IVersionProvider"/> extension methods.
    /// </summary>
    internal static class ProviderExtensionMethods
    {
        internal delegate IVersionProvider FilterCallback(IVersionProvider provider
            , Func<IVersionProvider, IVersionProvider> filter = null);

        /// <summary>
        /// Returns the <paramref name="provider"/> itself or the thing returned by the
        /// <paramref name="filter"/>.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static IVersionProvider Filter(this IVersionProvider provider
            , Func<IVersionProvider, IVersionProvider> filter = null)
        {
            filter = filter ?? delegate { return provider; };
            return filter.Invoke(provider);
        }
    }
}
