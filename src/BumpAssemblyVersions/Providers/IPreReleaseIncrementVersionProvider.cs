namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    /// <inheritdoc />
    public interface IPreReleaseIncrementVersionProvider : IVersionProvider
    {
        /// <summary>
        /// Gets or sets the Pre-Release Label.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Gets or sets the Pre-Release Value Width.
        /// </summary>
        int ValueWidth { get; set; }

        /// <summary>
        /// Gets whether ShouldDiscard. This property has precedence over Version Bumping.
        /// </summary>
        bool ShouldDiscard { get; }
    }
}
