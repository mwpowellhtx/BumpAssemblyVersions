﻿namespace Bav
{
    ///<summary>
    /// The No Op may optionally Reset depending on the conditions of
    /// <see cref="VersionProviderBase.MayReset"/> and
    /// <see cref="VersionProviderBase.MoreSignificantProviders"/>.
    /// </summary>
    /// <inheritdoc />
    public class NoOpVersionProvider : VersionProviderBase
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "None";

        /// <summary>
        /// Performs the Provider Change Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = ShouldReset ? "0" : current) != current;

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private NoOpVersionProvider(NoOpVersionProvider other)
            : base(other)
        {
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal NoOpVersionProvider()
        {
        }
    }
}
