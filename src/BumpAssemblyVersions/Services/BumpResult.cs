using System;

namespace Bav
{
    using static String;

    /// <summary>
    /// 
    /// </summary>
    public class BumpResult
    {
        internal Type AttributeType { get; set; }

        internal string Line { get; set; }

        internal string AttributeName { private get; set; }

        internal string Original => $"[assembly: {AttributeName}(\"{OldVersionAndSemanticString}\")]";

        internal string Result => $"[assembly: {AttributeName}(\"{VersionAndSemanticString}\")]";

        /// <summary>
        /// Whether DidBump is a reflection of whether <see cref="VersionString"/>
        /// <see cref="IsNullOrEmpty"/>.
        /// </summary>
        internal bool DidBump => !IsNullOrEmpty(VersionString);

        internal string OldVersionString { private get; set; }

        internal string OldSemanticString { private get; set; }

        internal string OldVersionAndSemanticString
            => BuildVersionAndSemanticString(OldVersionString, OldSemanticString);

        // TODO: TBD: may or may not need/want to maintain Version Element separation until the last Build moment...
        internal string VersionString { private get; set; }

        internal string SemanticString { private get; set; }

        internal string VersionAndSemanticString
            => BuildVersionAndSemanticString(VersionString, SemanticString);

        private static string BuildVersionAndSemanticString(string versionString, string semanticString, char hyp = '-')
            => IsNullOrEmpty(semanticString) ? versionString : $"{versionString}{hyp}{semanticString}";
    }
}
