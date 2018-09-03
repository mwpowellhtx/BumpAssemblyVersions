using System;
using System.Collections.Generic;
using System.Linq;

namespace Bav
{
    using static String;

    /// <summary>
    /// Defines the nature of the Multipart <see cref="VersionProviderBase.Timestamp"/> based
    /// callback. All other rules concerning whether <see cref="VersionProviderBase.UseUtc"/>
    /// still apply.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public delegate string MultipartTimestampCallback(DateTime timestamp);

    /// <summary>
    /// The success of this Provider hingeson the dynamics of <see cref="MultipartCallbacks"/>
    /// having been specified depending on the use case.
    /// </summary>
    /// <inheritdoc cref="VersionProviderBase" />
    /// <inheritdoc cref="IMultipartVersionProvider" />
    public abstract class MultipartTimestampVersionProviderBase
        : VersionProviderBase
            , IMultipartVersionProvider
    {
        /// <summary>
        /// Override with the Callbacks supported by this Provider.
        /// </summary>
        protected abstract IEnumerable<MultipartTimestampCallback> MultipartCallbacks { get; }

        /// <summary>
        /// Returns whether the attempted to Try to Change the Version element actually did.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = Join(Empty, MultipartCallbacks.Select(
                             callback => callback(ProtectedTimestamp)))) != current;

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        protected MultipartTimestampVersionProviderBase(MultipartTimestampVersionProviderBase other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Protected Default Constructor.
        /// </summary>
        /// <inheritdoc />
        protected MultipartTimestampVersionProviderBase()
        {
        }
    }
}
