using System;
using System.Linq;

namespace Bav
{
    using static String;

    /// <summary>
    /// Pre-Release is not unlike the <see cref="IncrementVersionProvider"/> excepting that
    /// a <see cref="Label"/> is also incorporated. The Version Provider may additionally be
    /// incorporated into the overall Version string a bit differently as well. Label may be
    /// set to virtually anything, but defaults to &quot;dev&quot; and is always transformed to
    /// lowercase and to alpha characters only. Any non-alpha characters will be stripped from
    /// the value Label.
    /// </summary>
    /// <inheritdoc />
    public class PreReleaseIncrementVersionProvider : IncrementVersionProvider
    {
        private const string DefaultLabel = "dev";

        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Pre-Release Increment";

        private string _label = DefaultLabel;

        private static string FilterLabel(string value)
            => (IsNullOrEmpty(value) ? Empty : value)
                .ToLower().Where(c => c >= 'a' && c <= 'z')
                .Aggregate(Empty, (label, c) => label + $"{c}");

        /// <summary>
        /// Gets or sets the Pre-Release Label.
        /// </summary>
        public string Label
        {
            get => _label;
            set => _label = FilterLabel(value ?? DefaultLabel);
        }

        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        private PreReleaseIncrementVersionProvider(PreReleaseIncrementVersionProvider other)
            : base(other)
        {
        }

        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        internal PreReleaseIncrementVersionProvider()
        {
        }
    }
}
