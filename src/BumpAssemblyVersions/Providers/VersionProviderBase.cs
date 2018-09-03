using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bav
{
    using static Convert;
    using static Type;
    using static BindingFlags;

    //AssemblyVersionAttribute
    //AssemblyFileVersionAttribute
    //AssemblyInformationalVersionAttribute -> may be based on AssemblyVersionAttribute or AssemblyFileVersionAttribute / or based on user-provided AssemblyInformationalVersionAttribute

    /// <inheritdoc cref="IVersionProvider" />
    /// <inheritdoc cref="ICloneable" />
    public abstract class VersionProviderBase : IVersionProvider
    {
        /// <summary>
        /// Gets whether the Provider is ForInternalUseOnly.
        /// </summary>
        /// <inheritdoc />
        public virtual bool ForInternalUseOnly { get; } = false;

        /// <summary>
        /// Gets the Id of the Provider.
        /// </summary>
        /// <inheritdoc />
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the Name of the Provider.
        /// </summary>
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <summary>
        /// Gets the Timestamp when the Provider is invoked.
        /// </summary>
        /// <inheritdoc />
        public DateTime Timestamp { get; internal set; }

        /// <summary>
        /// Gets whether to UseUtc when performing the Change.
        /// </summary>
        /// <inheritdoc />
        public bool UseUtc { get; private set; }

        private DateTime? _protectedTimestamp;

        /// <summary>
        /// Gets the ProtectedTimeStamp. Basically, may be Universal or not depending upon
        /// <see cref="UseUtc"/>.
        /// </summary>
        protected DateTime ProtectedTimestamp
            => _protectedTimestamp
               ?? (_protectedTimestamp = UseUtc ? Timestamp.ToUniversalTime() : Timestamp).Value;

        /// <summary>
        /// Sets the <see cref="Timestamp"/> and <see cref="UseUtc"/> settings. Also resets the
        /// <see cref="ProtectedTimestamp"/>.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="useUtc"></param>
        /// <inheritdoc />
        public void SetTimestamp(DateTime timestamp, bool? useUtc = null)
        {
            Timestamp = timestamp;
            UseUtc = useUtc ?? false;
            _protectedTimestamp = null;
            _midnightTimestamp = null;
        }

        /// <summary>
        /// Signals whether the Provider Changed the Version element.
        /// </summary>
        /// <inheritdoc />
        public bool Changed { get; protected set; }

        /// <summary>
        /// Captures a set of MoreSignificantProviders in terms of their element position
        /// in the overall Version. This is used during some Strategies to determine whether
        /// <see cref="MayReset"/>.
        /// </summary>
        /// <inheritdoc />
        public IEnumerable<IVersionProvider> MoreSignificantProviders { get; internal set; }

        /// <summary>
        /// Gets or sets whether MayReset.
        /// </summary>
        /// <inheritdoc />
        public bool MayReset { get; set; }

        /// <summary>
        /// Gets whether Should Reset. Should Reset is stronger than <see cref="MayReset"/>
        /// and incorporates the <see cref="MoreSignificantProviders"/> for their
        /// <see cref="IVersionProvider.Changed"/> bits. Note that there must also be
        /// <see cref="MoreSignificantProviders"/> for this to make a difference.
        /// </summary>
        protected bool ShouldReset
            => MayReset
               && MoreSignificantProviders.Any()
               && MoreSignificantProviders.Any(provider => provider.Changed);

        /// <summary>
        /// Override in order to Provide the Version Change strategy.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public abstract bool TryChange(string current, out string result);

        private DateTime? _midnightTimestamp;

        /// <summary>
        /// Gets the <see cref="ProtectedTimestamp"/> at Midnight.
        /// </summary>
        protected DateTime MidnightTimestamp
            => _midnightTimestamp ?? (_midnightTimestamp = new DateTime(
                   ProtectedTimestamp.Year, ProtectedTimestamp.Month, ProtectedTimestamp.Day)).Value;

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Default Protected Constructor.
        /// </summary>
        protected VersionProviderBase()
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        /// Protected Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        protected VersionProviderBase(VersionProviderBase other)
        {
            SetTimestamp(other.Timestamp, other.UseUtc);
            MayReset = other.MayReset;
        }

        /// <summary>
        /// Returns a Clone of this object.
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="ICloneable" />
        public object Clone()
        {
            var providerType = GetType();
            var ctor = providerType.GetConstructor(Instance | NonPublic, DefaultBinder, new[] {providerType}, null);
            return ChangeType(ctor?.Invoke(new object[] {this}), providerType);
        }
    }
}
