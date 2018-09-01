using System;

namespace Bav
{
    /// <inheritdoc cref="BumpResult" />
    /// <see cref="BumpResult"/>
    /// <see cref="IAssemblyInfoBumpResult"/>
    public class AssemblyInfoBumpResult : BumpResult, IAssemblyInfoBumpResult
    {
        /// <inheritdoc />
        public string Original => GetResultPattern(OldVersionAndSemanticString);

        /// <inheritdoc />
        public string Result => GetResultPattern(VersionAndSemanticString);

        /// <inheritdoc />
        public string Line { get; set; }

        /// <inheritdoc />
        public Type AttributeType { get; set; }

        /// <inheritdoc />
        public string AttributeName { private get; set; }

        private string GetResultPattern(string versionAndSemantic)
            => $"[assembly: {AttributeName}(\"{versionAndSemantic}\")]";

        /// <summary>
        /// Internal Constructor.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <inheritdoc />
        internal AssemblyInfoBumpResult(IBumpVersionDescriptor descriptor)
            : base(descriptor)
        {
        }
    }
}
