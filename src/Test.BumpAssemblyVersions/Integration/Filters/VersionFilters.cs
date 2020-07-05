using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bav
{
    using static String;
    using static RegexOptions;

    /// <summary>
    /// Represents Version Filter concerns.
    /// </summary>
    public interface IVersionFilter
    {
        /// <summary>
        /// Tries to Extract the Version given <paramref name="sourceText"/>.
        /// Result lands in the <paramref name="s"/> output parameter.
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        bool TryExtractVersion(string sourceText, out string s);
    }

    // ReSharper disable once UnusedTypeParameter
    /// <summary>
    /// Represents Assembly Attribute <see cref="IVersionFilter"/> for the
    /// <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    public interface IAssemblyAttributeVersionFilter<T> : IVersionFilter
        where T : Attribute
    {
    }

    /// <summary>
    /// Represents <see cref="IVersionFilter"/> concerns related to CSharp Xml Project file.
    /// </summary>
    /// <inheritdoc />
    public interface IProjectXmlVersionFilter : IVersionFilter
    {
        /// <summary>
        /// Gets or sets the <see cref="VersionKind"/>.
        /// </summary>
        VersionKind Kind { get; set; }
    }

    /// <summary>
    /// <see cref="IVersionFilter"/> base class.
    /// </summary>
    /// <inheritdoc />
    public abstract class VersionFilter : IVersionFilter
    {
        /// <summary>
        /// Version Pattern constant.
        /// </summary>
        protected const string VersionPattern = @"(?<s>[\d\.\-\*a-zA-Z]+)";

        /// <summary>
        /// Filter Regular Expression Pattern.
        /// </summary>
        protected abstract string FilterRegexPattern { get; }

        private Regex _filterRegex;

        private Regex FilterRegex => _filterRegex ?? (_filterRegex = new Regex(FilterRegexPattern, Compiled));

        /// <inheritdoc />
        public bool TryExtractVersion(string sourceText, out string s)
        {
            // TODO: TBD: may implement using the NuGet Versioning...
            s = null;
            var match = sourceText.Replace("\r\n", "\n").Split('\n')
                .Select(line => FilterRegex.Match(line.Trim()))
                .SingleOrDefault(m => m.Success);
            return match?.Success == true
                   && match.Groups.HasGroupName(nameof(s))
                   && !IsNullOrEmpty(s = match.Groups[nameof(s)].Value);
        }

        private string _s;

        /// <summary>
        /// Returns the String Representation of the <see cref="IVersionFilter"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => _s
               ?? (_s = $"{GetType().Name}: {{{{ {FilterRegex} }}}}");
    }

    /// <summary>
    /// <see cref="VersionFilter"/> concerning Assembly Attribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc cref="VersionFilter"/>
    public abstract class AssemblyAttributeVersionFilter<T> : VersionFilter, IAssemblyAttributeVersionFilter<T>
        where T : Attribute
    {
    }

    /// <summary>
    /// Represents the <see cref="AssemblyAttributeVersionFilter{T}"/> using the Short
    /// <see cref="Attribute"/> naming convention.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc cref="AssemblyAttributeVersionFilter{T}" />
    public class ShortAssemblyAttributeVersionFilter<T> : AssemblyAttributeVersionFilter<T>
        where T : Attribute
    {
        private string _filterRegexPattern;

        /// <inheritdoc />
        protected override string FilterRegexPattern
            => _filterRegexPattern
               ?? (_filterRegexPattern = $@"^\[assembly\: {typeof(T).ToShortName()}\(""{VersionPattern}""\)\]$");
    }

    /// <summary>
    /// Represents the <see cref="AssemblyAttributeVersionFilter{T}"/> using the Long
    /// <see cref="Attribute"/> naming convention.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <inheritdoc />
    internal class LongAssemblyAttributeVersionFilter<T> : AssemblyAttributeVersionFilter<T>
        where T : Attribute
    {
        private string _filterRegexPattern;

        /// <inheritdoc />
        protected override string FilterRegexPattern
            => _filterRegexPattern
               ?? (_filterRegexPattern = $@"^\[assembly\: {typeof(T).ToLongName()}\(""{VersionPattern}""\)\]$");
    }

    /// <summary>
    /// Represents the <see cref="VersionFilter"/> for CSharp Xml Project file.
    /// </summary>
    /// <inheritdoc cref="VersionFilter" />
    internal class ProjectXmlVersionFilter : VersionFilter, IProjectXmlVersionFilter
    {
        public VersionKind Kind { get; set; }

        private string _filterRegexPattern;

        private static string GetElement(string name, string value) => $@"\<{name}\>{value}\<\/{name}\>";

        /// <inheritdoc />
        protected override string FilterRegexPattern
            => _filterRegexPattern
               ?? (_filterRegexPattern = $@"^{GetElement($"{Kind}", VersionPattern)}$");
    }
}
