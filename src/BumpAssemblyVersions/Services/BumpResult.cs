using System;

namespace Bav
{
    using static String;

    /// <inheritdoc />
    public abstract class BumpResult : IBumpResult
    {
        /// <inheritdoc />
        public IBumpVersionDescriptor Descriptor { get; private set; }

        /// <summary>
        /// Whether DidBump is a reflection of whether <see cref="VersionString"/>
        /// <see cref="IsNullOrEmpty"/>.
        /// </summary>
        /// <inheritdoc />
        public bool DidBump => !IsNullOrEmpty(VersionString);

        /// <inheritdoc />
        public string OldVersionString { private get; set; }

        /// <inheritdoc />
        public string OldSemanticString { private get; set; }

        /// <inheritdoc />
        public string OldVersionAndSemanticString => BuildVersionAndSemanticString(OldVersionString, OldSemanticString);

        // TODO: TBD: may or may not need/want to maintain Version Element separation until the last Build moment...
        /// <inheritdoc />
        public string VersionString { private get; set; }

        /// <inheritdoc />
        public string SemanticString { private get; set; }

        /// <inheritdoc />
        public string VersionAndSemanticString => BuildVersionAndSemanticString(VersionString, SemanticString);

        private static string BuildVersionAndSemanticString(string versionString, string semanticString, char hyp = '-')
            => IsNullOrEmpty(semanticString) ? versionString : $"{versionString}{hyp}{semanticString}";

        /// <summary>
        /// Protected Constructor.
        /// </summary>
        /// <param name="descriptor"></param>
        protected BumpResult(IBumpVersionDescriptor descriptor)
        {
            Descriptor = descriptor;
        }
    }
}
