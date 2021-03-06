﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using static Int32;
    using static Math;
    using static String;
    using static RegexOptions;
    using static StringComparison;

    /// <summary>
    /// Pre-Release is not unlike the <see cref="IncrementVersionProvider"/> excepting that
    /// a <see cref="Label"/> is also incorporated. The Version Provider may additionally be
    /// incorporated into the overall Version string a bit differently as well. Label may be
    /// set to virtually anything, but defaults to &quot;dev&quot; and is always transformed to
    /// lowercase and to alpha characters only. Any non-alpha characters will be stripped from
    /// the value <see cref="Label"/>. The provider basically conforms to the Semantic Versioning
    /// Rule 9 concerning Pre-Release Version. This particular Provider operates on the
    /// Pre-Release element itself fully apart from the hyphen delimiter itself. Additionally, we
    /// are considering this to be just one, single Identifier as far as the Semantic Versioning
    /// specification is concerned.
    /// </summary>
    /// <inheritdoc cref="IncrementVersionProvider" />
    /// <inheritdoc cref="IPreReleaseIncrementVersionProvider" />
    /// <see cref="!:http://semver.org/"/>
    public class PreReleaseIncrementVersionProvider
        : IncrementVersionProvider
            , IPreReleaseIncrementVersionProvider
    {
        /// <summary>
        /// Gets the Provider Name.
        /// </summary>
        /// <inheritdoc />
        public override string Name { get; } = "Pre-Release Increment";

        private string _label = Empty;

        private static string FilterLabel(string value)
            => (IsNullOrEmpty(value) ? Empty : value)
                .ToLower().Where(c => c >= 'a' && c <= 'z')
                .Aggregate(Empty, (label, c) => label + $"{c}");

        /// <inheritdoc />
        public string Label
        {
            get => _label;
            set => _label = FilterLabel(value ?? Empty);
        }

        private const int ValueWidthMinimum = 1;

        private const int DefaultValueWidth = 3;

        private int _valueWidth;

        /// <inheritdoc />
        public int ValueWidth
        {
            get => _valueWidth;
            set => _valueWidth = Max(ValueWidthMinimum, value);
        }

        /// <inheritdoc />
        public virtual bool ShouldDiscard { get; internal set; }

        // ReSharper disable once SuggestBaseTypeForParameter, UnusedMember.Local
        /// <summary>
        /// Private Copy Constructor.
        /// </summary>
        /// <param name="other"></param>
        /// <inheritdoc />
        private PreReleaseIncrementVersionProvider(PreReleaseIncrementVersionProvider other)
            : base(other)
        {
            Copy(other);
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Internal Default Constructor.
        /// </summary>
        /// <inheritdoc />
        internal PreReleaseIncrementVersionProvider() => Initialize();

        /// <summary>
        /// Initializes the Version Provider.
        /// </summary>
        private void Initialize()
        {
            ValueWidth = DefaultValueWidth;
            ShouldDiscard = false;
        }

        private void Copy(IPreReleaseIncrementVersionProvider other)
        {
            Label = other.Label;
            ValueWidth = other.ValueWidth;
            ShouldDiscard = other.ShouldDiscard;
        }

        private static readonly Regex ElementRegex = new Regex(
            @"^(?<label>[a-zA-Z]*)(?<value>\d+)$", Compiled);

        private string ParseAndIncrement(string current)
        {
            // TODO: TBD: may strip out dot-delimited Identifier parts beyond the first...

            // Basically, extract the Value itself from whatever overall Element may be given.
            var match = ElementRegex.Match(current);

            const int one = 1;

            // Specify a little shorthand for case insensitive String Equality.
            bool StringEquals(string a, string b)
                => string.Equals(a, b, CurrentCultureIgnoreCase);

            // In this case Reset when the Label has Changed.
            bool MustReset(GroupCollection groups)
            {
                const string label = nameof(label);
                return groups.HasGroupName(label)
                       && !StringEquals(groups[label].Value, Label);
            }

            // Gets the next Increment with a Minimum of One in the case of Overflow.
            int Get(Capture cap) => Increment(
                Parse(cap.Value), one, (int) Pow(10d, ValueWidth) - 1);

            const string value = nameof(value);

            const int zed = 0;

            // Rule out the Match up front.
            var result = (ShouldReset
                          || MustReset(match.Groups)
                          || !(match.Success && match.Groups.HasGroupName(value))
                    ? $"{one}"
                    : $"{Get(match.Groups[value])}")
                .PadLeft(ValueWidth, $"{zed}");

            return result;
        }

        /// <summary>
        /// Tries to Change the Version Bumping according to the instructions for the Provider.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override bool TryChange(string current, out string result)
            => Changed = (result = ShouldDiscard
                             ? Empty
                             : $"{Label ?? Empty}{ParseAndIncrement(current)}"
                                 .TrimLeading('0')) != current;
    }
}
