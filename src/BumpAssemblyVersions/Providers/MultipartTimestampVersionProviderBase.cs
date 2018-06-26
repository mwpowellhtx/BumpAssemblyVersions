﻿using System;
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
    /// <inheritdoc />
    public abstract class MultipartTimestampVersionProviderBase : VersionProviderBase
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

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter
        protected MultipartTimestampVersionProviderBase(MultipartTimestampVersionProviderBase other)
            : base(other)
        {
        }

        /// <summary>
        /// Protected Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        protected MultipartTimestampVersionProviderBase()
        {
        }
    }
}
