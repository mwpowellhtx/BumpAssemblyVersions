namespace Bav
{
    /// <summary>
    /// Provides some details concerning Project based Bump Results.
    /// </summary>
    /// <inheritdoc />
    public interface IProjectBumpResult : IBumpResult
    {
        /// <summary>
        /// Gets or sets the ProtectedElementName.
        /// </summary>
        string ProtectedElementName { get; set; }
    }
}
