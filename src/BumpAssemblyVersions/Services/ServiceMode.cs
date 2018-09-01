using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum ServiceMode
    {
        /// <summary>
        /// 
        /// </summary>
        NoElements = 0,

        /// <summary>
        /// 
        /// </summary>
        VersionElements = 1,

        /// <summary>
        /// 
        /// </summary>
        ReleaseElements = 1 << 1,

        /// <summary>
        /// 
        /// </summary>
        MetadataElements = 1 << 2,

        /// <summary>
        /// 
        /// </summary>
        VersionAndReleaseElements = VersionElements | ReleaseElements,

        /// <summary>
        /// 
        /// </summary>
        AllElements = VersionElements | ReleaseElements | MetadataElements
    }
}
