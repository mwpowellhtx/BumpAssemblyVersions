namespace Bav
{
    /// <summary>
    /// Represents the <see cref="BumpResult"/>.
    /// </summary>
    public interface IBumpResult
    {
        /// <summary>
        /// Gets the Descriptor.
        /// </summary>
        IBumpVersionDescriptor Descriptor { get; }

        /// <summary>
        /// Sets the OldVersionString.
        /// </summary>
        string OldVersionString { set; }

        /// <summary>
        /// Sets the OldSemanticString.
        /// </summary>
        string OldSemanticString { set; }

        /// <summary>
        /// Gets the OldVersionAndSemanticString.
        /// </summary>
        /// <see cref="OldVersionString"/>
        /// <see cref="OldSemanticString"/>
        string OldVersionAndSemanticString { get; }

        /// <summary>
        /// Sets the VersionString.
        /// </summary>
        string VersionString { set; }

        /// <summary>
        /// Sets the SemanticString.
        /// </summary>
        string SemanticString { set; }

        /// <summary>
        /// Gets the VersionAndSemanticString.
        /// </summary>
        string VersionAndSemanticString { get; }

        /// <summary>
        /// Gets whether DidBump.
        /// </summary>
        bool DidBump { get; }
    }
}
