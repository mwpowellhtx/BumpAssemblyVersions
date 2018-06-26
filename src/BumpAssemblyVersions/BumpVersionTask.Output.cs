namespace Bav
{
    using Microsoft.Build.Framework;

    public partial class BumpVersion
    {
        /// <summary>
        /// 
        /// </summary>
        [Output]
        public string NewVersion { get; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Output]
        public string NewPackageVersion { get; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Output]
        public string NewBavNewAssemblyVersion { get; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Output]
        public string NewBavNewFileVersion { get; } = string.Empty;
    }
}
