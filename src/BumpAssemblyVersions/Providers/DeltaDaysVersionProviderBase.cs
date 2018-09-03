using System;

namespace Bav
{
    using static Int16;

    /// <summary>
    /// Changes the Version element based on the number of days elapsed since
    /// <see cref="BaseTimestamp"/>.
    /// </summary>
    /// <inheritdoc />
    public abstract class DeltaDaysVersionProviderBase : VersionProviderBase
    {
        /// <summary>
        /// Override in order to provide the BaseTimestamp.
        /// </summary>
        protected abstract DateTime BaseTimestamp { get; }

        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public sealed override string Name => $"Days Since {BaseTimestamp:d}";

        /// <summary>
        /// Performs the Provider Strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public sealed override bool TryChange(string current, out string result)
            => Changed = (result = $"{((ProtectedTimestamp - BaseTimestamp).Days + 1) % MaxValue}") != current;

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Protected Default Constructor.
        /// </summary>
        /// <inheritdoc />
        protected DeltaDaysVersionProviderBase()
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        /// Protected Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        protected DeltaDaysVersionProviderBase(DeltaDaysVersionProviderBase other)
            : base(other)
        {
        }
    }
}
