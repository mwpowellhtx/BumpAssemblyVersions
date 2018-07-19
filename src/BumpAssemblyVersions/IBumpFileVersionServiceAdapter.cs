namespace Bav
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBumpFileVersionServiceAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="bumpVersionServices"></param>
        /// <returns></returns>
        bool TryBumpVersions(string fullPath, params IAssemblyInfoBumpVersionService[] bumpVersionServices);
    }
}
