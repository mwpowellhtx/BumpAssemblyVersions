using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bav
{
    using static File;
    using static Path;
    using static String;

    /* TODO: TBD: thinking that the Source, one file, whatever that one file is, whether
     * Project or Assy Info, is useful one time for one iteration, whereas the same
     * read-string should be used for potentially one or more version filters. */
    /// <summary>
    /// Represents a basic Filter Source.
    /// </summary>
    public interface IFilterSource
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Gets or sets the ProjectFullPath.
        /// </summary>
        string ProjectFullPath { get; set; }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Gets the SourceText.
        /// </summary>
        string SourceText { get; }

        /// <summary>
        /// Gets the Versions.
        /// </summary>
        IEnumerable<string> Versions { get; }
    }

    /// <summary>
    /// Represents a <see cref="IFilterSource"/> pertaining to Assembly Attribute Versioning.
    /// </summary>
    /// <inheritdoc />
    public interface IAssemblyAttributeVersionFilterSource : IFilterSource
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Gets or sets the RelativePath.
        /// </summary>
        string RelativePath { get; set; }
    }

    /// <summary>
    /// Repreesnts a <see cref="IFilterSource"/> pertaining to the CSharp Xml Project document.
    /// </summary>
    /// <inheritdoc />
    public interface IProjectXmlVersionFitlerSource : IFilterSource
    {
    }

    /// <summary>
    /// Base class for <see cref="IFilterSource"/> concerns.
    /// </summary>
    /// <inheritdoc />
    public abstract class FilterSource : IFilterSource
    {
        /// <inheritdoc />
        public string ProjectFullPath { get; set; }

        /// <inheritdoc />
        public abstract string SourceText { get; }

        /// <summary>
        /// Tries to Read the <see cref="SourceText"/> given <paramref name="fullPath"/>.
        /// Result lands in the <paramref name="s"/> output parameter.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        protected static bool TryReadSourceText(string fullPath, out string s)
        {
            const FileMode mode = FileMode.Open;
            const FileAccess access = FileAccess.Read;
            const FileShare share = FileShare.Read;

            using (var fs = Open(fullPath, mode, access, share))
            {
                using (var sr = new StreamReader(fs))
                {
                    s = sr.ReadToEnd();
                }
            }

            return !IsNullOrEmpty(s);
        }

        private IEnumerable<IVersionFilter> Filters { get; }

        /// <summary>
        /// Protected Constructor
        /// </summary>
        /// <param name="filters"></param>
        protected FilterSource(IEnumerable<IVersionFilter> filters)
        {
            Filters = filters.ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<string> Versions
        {
            get
            {
                // Capture the Source Once and Once Only.
                var sourceText = SourceText;
                // Then Ply that Source across the range of Filters.
                return Filters.Select(filter => filter.TryExtractVersion(sourceText, out var s) ? s : Empty);
            }
        }
    }

    /// <summary>
    /// Represents <see cref="FilterSource"/> concerns for <see cref="Attribute"/>.
    /// </summary>
    /// <inheritdoc cref="FilterSource"/>
    internal class AssemblyAttributeVersionFilterSource : FilterSource, IAssemblyAttributeVersionFilterSource
    {
        internal AssemblyAttributeVersionFilterSource(params IVersionFilter[] filters)
            : base(filters)
        {
        }

        public string RelativePath { get; set; }

        // ReSharper disable once AssignNullToNotNullAttribute
        private string AssemblyInfoFullPath => Combine(GetDirectoryName(ProjectFullPath), RelativePath);

        private string _sourceText;

        public override string SourceText
            => TryReadSourceText(AssemblyInfoFullPath, out _sourceText)
                ? _sourceText
                : Empty;

        private string _s;

        public override string ToString() => _s ?? (_s = $"{GetType().Name}: '{AssemblyInfoFullPath}'");
    }

    /// <summary>
    /// Represents <see cref="FilterSource"/> concerns for CSharp Xml Project.
    /// </summary>
    /// <inheritdoc cref="FilterSource"/>
    internal class ProjectXmlVersionFilterSource : FilterSource, IProjectXmlVersionFitlerSource
    {
        internal ProjectXmlVersionFilterSource(params IVersionFilter[] filters)
            : base(filters)
        {
        }

        private string _sourceText;

        public override string SourceText
            => TryReadSourceText(ProjectFullPath, out _sourceText)
                ? _sourceText
                : Empty;

        private string _s;

        public override string ToString() => _s ?? (_s = $"{GetType().Name}: '{ProjectFullPath}'");
    }
}
