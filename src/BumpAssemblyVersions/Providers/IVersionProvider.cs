using System;
using System.Collections.Generic;

namespace Bav
{
    /// <summary>
    /// Version Provider defines a Strategy through which a Version element may
    /// <see cref="TryChange"/>.
    /// </summary>
    public interface IVersionProvider
    {
        /// <summary>
        /// 
        /// </summary>
        bool ForInternalUseOnly { get; }

        /// <summary>
        /// Gets the Provider Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the Name of the Version Provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Timestamp.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets whether <see cref="Timestamp"/> ought to be used as
        /// <see cref="DateTime.ToUniversalTime"/>. This may be specified individually for each
        /// specific Version Element, as in MajorUseUtc, MinorUseUtc, PatchUseUtc, BuildUseUtc,
        /// and so on, or for the entire Version as in UseUtc. The default is false, and the
        /// specific Version Element overrides the Version setting.
        /// </summary>
        bool UseUtc { get; }

        /// <summary>
        /// Sets the <see cref="Timestamp"/> and whether to <see cref="UseUtc"/>.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="useUtc"></param>
        void SetTimestamp(DateTime timestamp, bool? useUtc = null);

        /// <summary>
        /// Gets or sets whether May Reset. If the Version Provider supports resetting, that is,
        /// is not based on dynamic sources such as <see cref="DateTime"/>, then Resets to Zero
        /// when any of the <see cref="MoreSignificantProviders"/> Changed.
        /// </summary>
        bool MayReset { get; set; }

        /// <summary>
        /// Returns whether the Provider Changed.
        /// </summary>
        bool Changed { get; }

        /// <summary>
        /// Gets the More Significant <see cref="IVersionProvider"/> instances.
        /// </summary>
        IEnumerable<IVersionProvider> MoreSignificantProviders { get; }

        /// <summary>
        /// Returns the <paramref name="current"/> value after applying the Change strategy. May
        /// utilize the <see cref="Timestamp"/> depending on the strategy, also depending upon
        /// <see cref="UseUtc"/>.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns>Whether the <paramref name="result"/> did in fact Change.</returns>
        bool TryChange(string current, out string result);
    }
}
