using System;

namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    internal enum ServiceMode
    {
        NoElements = 0,
        VersionElements = 1,
        ReleaseElements = 1 << 1,
        MetadataElements = 1 << 2,
        VersionAndReleaseElements = VersionElements | ReleaseElements,
        AllElements = VersionElements | ReleaseElements | MetadataElements
    }
}
